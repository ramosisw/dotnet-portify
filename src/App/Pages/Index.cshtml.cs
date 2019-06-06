using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models.Spotify;
using Core.Models.Spotify.Me;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ISpotifyService _spotifyService;

        public IndexModel(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        public SpotifyToken Token { get; private set; }
        public MeMessage MeModel { get; private set; }
        public bool IsLogged { get; set; }

        public async Task OnGetAsync([FromQuery] SpotifyToken token)
        {
            Token = token;
            IsLogged = !(token == null || string.IsNullOrEmpty(token?.AccessToken));
            MeModel = await _spotifyService.GetMeAsync(Token);
        }
    }
}
