using System.Collections.Generic;

namespace Core.Models.Spotify.Playlists
{
    public class GetPlaylists : SpotifyPagination
    {
        public List<PlaylistObject> Items { get; set; }

    }
}