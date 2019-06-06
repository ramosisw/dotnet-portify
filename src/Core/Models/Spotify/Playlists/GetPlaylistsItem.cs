namespace Core.Models.Spotify.Playlists
{
    public class GetPlaylistsItem : SpotifyUri
    {
        public bool Collaborative { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Public { get; set; }
    }
}