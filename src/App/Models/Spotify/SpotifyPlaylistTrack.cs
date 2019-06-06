using Core.Models.Spotify.Playlists;

namespace App.Models.Spotify
{
    public class SpotifyPlaylistTrack : GetPlaylistsTracksItem
    {
        public SpotifyPlaylistTrack(GetPlaylistsTracksItem track)
        {
            IsLocal = track.IsLocal;
            Track = track.Track;
        }
    }
}