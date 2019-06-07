using Newtonsoft.Json;

namespace Core.Models.Spotify.Playlists
{
    public class PostPlaylist
    {
        [JsonProperty("collaborative")]
        public bool Collaborative { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }
    }
}