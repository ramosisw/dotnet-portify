using Core.Models.Spotify.Tracks;
using Newtonsoft.Json;

namespace Core.Models.Spotify.Playlists
{
    public class GetPlaylistsTracksItem
    {
        [JsonProperty("is_local")]
        public bool IsLocal { get; set; }
        public GetTrack Track { get; set; }
    }
}