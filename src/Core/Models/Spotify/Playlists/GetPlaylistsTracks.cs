using System.Collections.Generic;

namespace Core.Models.Spotify.Playlists
{
    public class GetPlaylistsTracks : SpotifyPagination
    {
        public List<GetPlaylistsTracksItem> Items { get; set; }
    }
}