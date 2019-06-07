using System.Collections.Generic;

namespace App.Models.Spotify
{
    public class SpotifyData
    {
        public SpotifyDataVersion Version { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public List<SpotifyPlaylist> Playlists { get; set; }
        public List<SpotifyPlaylistTrack> Tracks { get; set; }
        public List<string> FollowPlaylists { get; set; }

        public SpotifyData()
        {
            Playlists = new List<SpotifyPlaylist>();
            Tracks = new List<SpotifyPlaylistTrack>();
            FollowPlaylists = new List<string>();
            Version = SpotifyDataVersion.VERSION_1;
        }
    }
}