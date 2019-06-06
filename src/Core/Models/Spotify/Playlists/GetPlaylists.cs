using System.Collections.Generic;

namespace Core.Models.Spotify.Playlists
{
    public class GetPlaylists : SpotifyPagination
    {
        public List<GetPlaylistsItem> Items { get; set; }

    }
}