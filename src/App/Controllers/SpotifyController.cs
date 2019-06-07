using Core.Models.Spotify.Playlists;
using Core.Models.Spotify.Tracks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Core.Models.Spotify;
using App.Models.Spotify;
using Newtonsoft.Json;
using Core.Services;
using System.IO;
using System;
using System.Collections.Generic;

namespace App.Controllers
{
    [Route("api/[controller]")]
    public class SpotifyController : Controller
    {
        private readonly ISpotifyService _spotifyService;

        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        public string Status()
        {
            return "API OK";
        }

        [HttpGet("authorization")]
        public IActionResult GetAuthorization()
        {
            return Redirect(_spotifyService.GetAuthorizationUrl(Guid.NewGuid().ToString()));
        }

        [HttpGet("callback")]
        public async Task<IActionResult> GetCallbackAsync([FromQuery] SpotifyCallback callback)
        {
            if (callback.Error == null)
            {
                var token = await _spotifyService.GetAuthorizationTokenAsync(callback.Code);
                return RedirectToPage("/Index", new { AccessToken = token.AccessToken });
            }

            return RedirectToPage($"/Index", new { Message = callback.Error, Type = "danger" });
        }

        [HttpGet("export")]
        public async Task<ActionResult<SpotifyData>> GetExportAsync([FromQuery] SpotifyToken token)
        {
            var hasMore = false;
            var offset = 0;

            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            var data = new SpotifyData();
            var user = await _spotifyService.GetMeAsync(token);
            if (string.IsNullOrEmpty(user.Id)) return RedirectToPage($"/Index", new { Message = "Invalid token, login!", Type = "warning" });
            data.UserId = user.Id;
            data.DisplayName = user.DisplayName;
            do
            {
                var playlists = await _spotifyService.GetPlaylistsAsync(token, offset);
                foreach (var playlist in playlists.Items)
                {
                    if (!playlist.Owner.Id.Equals(user.Id))
                    {
                        data.FollowPlaylists.Add(playlist.Uri);
                        continue;
                    }

                    var playlistItem = new SpotifyPlaylist(playlist);
                    var _continue = false;
                    offset = 0;
                    do
                    {
                        var playlistTracks = await _spotifyService.GetPlaylistsTracksAsync(token, playlist.Id, offset);
                        if (playlistTracks.Total == 0)
                        {
                            _continue = true; break;
                        }
                        foreach (var playlistTrack in playlistTracks.Items)
                        {
                            if (playlistTrack.IsLocal) continue; //ignore local
                            playlistItem.Tracks.Add(new SpotifyPlaylistTrack(playlistTrack.Track));
                        }
                        hasMore = !string.IsNullOrEmpty(playlistTracks.Next);
                        offset = playlistTracks.Offset + playlistTracks.Limit;
                    } while (hasMore);
                    if (_continue) continue;
                    data.Playlists.Add(playlistItem);
                }
                hasMore = !string.IsNullOrEmpty(playlists.Next);
                offset = playlists.Offset + playlists.Limit;
            } while (hasMore);

            offset = 0;
            do
            {
                var tracks = await _spotifyService.GetTracksAsync(token, offset);
                foreach (var track in tracks.Items)
                {
                    data.Tracks.Add(new SpotifyPlaylistTrack(track.Track));
                }
                hasMore = !string.IsNullOrEmpty(tracks.Next);
                offset = tracks.Offset + tracks.Limit;
            } while (hasMore);
            return data;
        }

        [HttpPost("import")]
        public async Task<ActionResult<bool>> PostImportAsync([FromQuery] SpotifyToken token, IFormFile importFile, string userId)
        {
            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            if (importFile == null || importFile?.Length == 0) return BadRequest();

            StreamReader reader = new StreamReader(importFile.OpenReadStream());
            string jsonData = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SpotifyData>(jsonData);

            if (data.Version != SpotifyDataVersion.VERSION_1) return RedirectToPage($"/Index", new { AccessToken = token.AccessToken, Message = "Unsuported version", Type = "warning" });
            var userPlaylists = await GetPlaylistsDictionaryAsync(token);

            foreach (var playlist in data.Playlists)
            {
                var playlistId = "";
                if (!userPlaylists.ContainsKey(playlist.Name))
                {
                    var playlistObject = await _spotifyService.PostPlaylistAsync(token, userId, new PostPlaylist
                    {
                        Collaborative = playlist.Collaborative,
                        Name = playlist.Name,
                        Description = playlist.Description,
                        Public = playlist.Public
                    });
                    userPlaylists.Add(playlistObject.Name, playlistObject.Id);
                }

                playlistId = userPlaylists[playlist.Name];

                var importTotalTracks = playlist.Tracks.Count;
                var userPlaylistTracks = await GetPlaylistTracksListAsync(token, playlistId);
                var importTotalTracksIterator = 0;
                do
                {
                    PostPlaylistTracks tracks = new PostPlaylistTracks();
                    for (var j = 0; (importTotalTracksIterator < importTotalTracks && j < 50); j++)
                    {
                        var track = playlist.Tracks[(importTotalTracksIterator++)];
                        if (userPlaylistTracks.Contains(track.Uri))
                        {
                            j--;
                            continue;
                        }
                        tracks.Uris.Add(track.Uri);
                    }
                    await _spotifyService.PostPlaylistTracksAsync(token, playlistId, tracks);
                } while (importTotalTracksIterator < importTotalTracks);
            }

            var userTracks = await GetTracksListAsync(token);

            var totalTracks = data.Tracks.Count;
            var i = 0;
            while (i < totalTracks)
            {
                var groupTracks = new List<string>();
                for (var j = 0; (i < totalTracks && j < 50); j++)
                {
                    var track = data.Tracks[(i++)];
                    if (userTracks.Contains(track.Uri))
                    {
                        j--;
                        continue;
                    }
                    groupTracks.Add(track.Id);
                }
                await _spotifyService.PutTracksAsync(token, new PutTracks
                {
                    Ids = string.Join(",", groupTracks.ToArray())
                });
            }

            foreach (var playlistFollow in data.FollowPlaylists)
            {
                var urisplit = playlistFollow.Split(":");
                if (urisplit.Length != 3) continue;
                var playlistId = urisplit[2];
                if (!await _spotifyService.IsFollowPlaylistAsync(token, playlistId))
                {
                    await _spotifyService.FollowPlaylistAsync(token, playlistId);
                }
            }

            return RedirectToPage($"/Index", new { Message = "Imported!", Type = "success" });
        }

        private async Task<IDictionary<string, string>> GetPlaylistsDictionaryAsync(SpotifyToken token)
        {
            var hasMore = false;
            var offset = 0;
            var result = new Dictionary<string, string>();
            do
            {
                var currentPlaylists = await _spotifyService.GetPlaylistsAsync(token, offset);
                foreach (var currentPlaylist in currentPlaylists.Items)
                    result.Add(currentPlaylist.Name, currentPlaylist.Id);
                hasMore = !string.IsNullOrEmpty(currentPlaylists.Next);
                offset = currentPlaylists.Offset + currentPlaylists.Limit;
            } while (hasMore);
            return result;
        }

        private async Task<List<string>> GetPlaylistTracksListAsync(SpotifyToken token, string playlistId)
        {
            var hasMore = false;
            var offset = 0;
            var result = new List<string>();
            do
            {
                var playlistTracks = await _spotifyService.GetPlaylistsTracksAsync(token, playlistId, offset);
                foreach (var playlistTrack in playlistTracks.Items)
                {
                    if (playlistTrack.IsLocal) continue; //ignore local
                    result.Add(playlistTrack.Track.Uri);
                }
                hasMore = !string.IsNullOrEmpty(playlistTracks.Next);
                offset = playlistTracks.Offset + playlistTracks.Limit;
            } while (hasMore);
            return result;
        }

        private async Task<List<string>> GetTracksListAsync(SpotifyToken token)
        {
            var hasMore = false;
            var offset = 0;
            var result = new List<string>();
            do
            {
                var tracks = await _spotifyService.GetTracksAsync(token, offset);
                foreach (var track in tracks.Items)
                    result.Add(track.Track.Uri);
                hasMore = !string.IsNullOrEmpty(tracks.Next);
                offset = tracks.Offset + tracks.Limit;
            } while (hasMore);
            return result;
        }
    }
}