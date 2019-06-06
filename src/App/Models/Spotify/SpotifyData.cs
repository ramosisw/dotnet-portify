using System.Collections.Generic;

namespace App.Models.Spotify
{
    public class SpotifyData
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public List<SpotifyPlaylist> Playlists { get; set; }

        public SpotifyData(){
            Playlists = new List<SpotifyPlaylist>();
        }
    }
}