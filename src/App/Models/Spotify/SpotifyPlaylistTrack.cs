using Core.Models.Spotify.Me;
using Core.Models.Spotify.Playlists;
using Core.Models.Spotify.Tracks;

namespace App.Models.Spotify
{
    public class SpotifyPlaylistTrack : TrackObject
    {
        public SpotifyPlaylistTrack() { }
        public SpotifyPlaylistTrack(TrackObject track)
        {
            Name = track.Name;
            PreviewUrl = track.PreviewUrl;
            Uri = track.Uri;
            Id = track.Id;
        }
    }
}