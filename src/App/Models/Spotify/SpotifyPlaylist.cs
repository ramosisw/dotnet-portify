using System.Collections.Generic;
using Core.Models.Spotify.Playlists;

namespace App.Models.Spotify
{
    public class SpotifyPlaylist : GetPlaylistsItem
    {

        public List<SpotifyPlaylistTrack> Tracks { get; set; }

        public SpotifyPlaylist()
        {
            Tracks = new List<SpotifyPlaylistTrack>();
        }

        public SpotifyPlaylist(GetPlaylistsItem playlist)
        {
            Collaborative = playlist.Collaborative;
            Id = playlist.Id;
            Name = playlist.Name;
            Public = playlist.Public;
            Uri = playlist.Uri;
            Tracks = new List<SpotifyPlaylistTrack>();
        }
    }
}