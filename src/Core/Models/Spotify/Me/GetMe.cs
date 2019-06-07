using Core.Models.Spotify.Users;
using Newtonsoft.Json;

namespace Core.Models.Spotify.Me
{
    public class GetMe : UserObject
    {
        public string Country { get; set; }
    }
}