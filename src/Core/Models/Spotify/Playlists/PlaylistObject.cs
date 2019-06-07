using Core.Models.Spotify.Users;

namespace Core.Models.Spotify.Playlists
{
    public class PlaylistObject : SpotifyUri
    {
        public bool Collaborative { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Public { get; set; }
        public UserObject Owner { get; set; }
    }
}