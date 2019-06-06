using Newtonsoft.Json;

namespace Core.Models.Spotify.Me
{
    public class MeMessage
    {

        public string Country { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Product { get; set; }
        public string Type { get; set; }
        public string Uri { get; set; }
    }
}