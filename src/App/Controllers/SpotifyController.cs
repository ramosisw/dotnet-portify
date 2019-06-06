using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using App.Models.Spotify;
using Core.Models.Spotify;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            var data = new SpotifyData();
            var user = await _spotifyService.GetMeAsync(token);
            data.UserId = user.Id;
            data.DisplayName = user.DisplayName;
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

        [HttpPost("import")]
        public async Task<ActionResult<bool>> PostImportAsync([FromQuery] SpotifyToken token, IFormFile importFile)
        {
            if (string.IsNullOrEmpty(token?.AccessToken)) return RedirectToAction(nameof(GetAuthorization));
            if (importFile == null || importFile?.Length == 0) return BadRequest();


            StreamReader reader = new StreamReader(importFile.OpenReadStream());
            string jsonData = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SpotifyData>(jsonData);
            foreach (var playlist in data.Playlists)
            {

            }

            return await Task.FromResult(true);
        }
    }
}