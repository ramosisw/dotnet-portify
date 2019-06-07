using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Models.Spotify.Playlists
{
    public class PostPlaylistTracks
    {
        [JsonProperty("uris")]
        public List<string> Uris { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        public PostPlaylistTracks() => Uris = new List<string>();
    }
}