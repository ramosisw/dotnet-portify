namespace Core.Models.Spotify
{
    public class SpotifyPagination
    {
        public string Href { get; set; }
        public int Limit { get; set; }
        public string Next { get; set; }
        public int Offset { get; set; }
        public string Previous { get; set; }
        public int Total { get; set; }
    }
}