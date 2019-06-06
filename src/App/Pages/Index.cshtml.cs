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

        public MeMessage MeModel { get; private set; }

        public async Task OnGetAsync(string token, string refresh_token)
        {
            MeModel = await _spotifyService.GetMeAsync(new SpotifyToken { AccessToken = token, RefreshToken = refresh_token });
        }
    }
}
