using Newtonsoft.Json;

namespace Core.Models.Spotify.Tracks
{
    public class TrackObject : SpotifyUri
    {
        public string Name { get; set; }

        public string Id { get; set; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }
    }
}