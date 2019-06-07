using Core.Models.Spotify.Playlists;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Core.Models.Spotify;
using App.Models.Spotify;
using Newtonsoft.Json;
using Core.Services;
using System.IO;
using System;

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
                return RedirectToPage("/Index", new { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
            }

            return RedirectToPage("/Index");
        }

        [HttpGet("export")]
        public async Task<ActionResult<SpotifyData>> GetExportAsync([FromQuery] SpotifyToken token)
        {
            var hasMore = false;
            var offset = 0;

            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            var data = new SpotifyData();
            var user = await _spotifyService.GetMeAsync(token);
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
            foreach (var playlist in data.Playlists)
            {
                var playlistObject = await _spotifyService.PostPlaylistAsync(token, userId, new PostPlaylist
                {
                    Collaborative = playlist.Collaborative,
                    Name = playlist.Name,
                    Description = playlist.Description,
                    Public = playlist.Public
                });

                var totalTracks = playlist.Tracks.Count;
                var i = 0;
                do
                {
                    PostPlaylistTracks tracks = new PostPlaylistTracks();
                    for (var j = 0; (i < totalTracks && j < 50); j++)
                    {
                        var track = playlist.Tracks[(i++)];
                        tracks.Uris.Add(track.Uri);
                    }
                    await _spotifyService.PostPlaylistTracksAsync(token, playlistObject.Id, tracks);
                } while (i < totalTracks);
            }

            return await Task.FromResult(true);
        }
    }
}