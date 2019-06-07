using System.Collections.Generic;

namespace Core.Models.Spotify.Me
{
    public class GetTracks : SpotifyPagination
    {
        public List<GetTracksItem> Items { get; set; }
    }
}