using Newtonsoft.Json;

namespace Core.Models.Spotify.Tracks
{
    public class GetTrack : SpotifyUri
    {
        public string Name { get; set; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }
    }
}