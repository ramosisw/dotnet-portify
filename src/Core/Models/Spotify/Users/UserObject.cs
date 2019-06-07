using Newtonsoft.Json;

namespace Core.Models.Spotify.Users
{
    public class UserObject : SpotifyUri
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Product { get; set; }
        public string Type { get; set; }
    }
}