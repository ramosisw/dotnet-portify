using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using App.Models.Spotify;
using Core.Models.Spotify;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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

        [Route("authorization")]
        public IActionResult GetAuthorization()
        {
            return Redirect(_spotifyService.GetAuthorizationUrl(Guid.NewGuid().ToString()));
        }

        [Route("callback")]
        public async Task<IActionResult> GetCallbackAsync([FromQuery] SpotifyCallback callback)
        {
            if (callback.Error == null)
            {
                var token = await _spotifyService.GetAuthorizationTokenAsync(callback.Code);
                return RedirectToPage("/Index", new { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
            }

            return RedirectToPage("/Index");
        }

        [Route("export")]
        public async Task<ActionResult<SpotifyData>> GetExportAsync([FromQuery] SpotifyToken token)
        {
            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            var data = new SpotifyData();
            var user = await _spotifyService.GetMeAsync(token);
            data.UserId = user.Id;
            var playlists = await _spotifyService.GetPlaylistsAsync(token);
            foreach (var playlist in playlists.Items)
            {
                var playlistItem = new SpotifyPlaylist(playlist);
                var tracks = await _spotifyService.GetPlaylistsTracksAsync(token, playlist.Id);
                if (tracks.Total == 0) continue;
                foreach (var track in tracks.Items)
                {
                    playlistItem.Tracks.Add(new SpotifyPlaylistTrack(track));
                }
                data.Playlists.Add(playlistItem);
            }
            return data;
        }
    }
}