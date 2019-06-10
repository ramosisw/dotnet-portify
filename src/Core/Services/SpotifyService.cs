using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using Core.Models.Spotify;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Core.Models.Spotify.Me;
using Core.Models.Spotify.Playlists;
using Core.Extensions;
using Core.Models.Spotify.Tracks;

namespace Core.Services
{
    public interface ISpotifyService
    {
        string GetAuthorizationUrl(string state);
        Task<SpotifyToken> GetAuthorizationTokenAsync(string code);
        Task<GetMe> GetMeAsync(SpotifyToken token);
        Task<GetTracks> GetTracksAsync(SpotifyToken token, int offset = 0, int limit = 50);
        Task<GetPlaylists> GetPlaylistsAsync(SpotifyToken token, int offset = 0, int limit = 50);
        Task<GetPlaylistsTracks> GetPlaylistsTracksAsync(SpotifyToken token, string playlistId, int offset = 0, int limit = 50);
        Task<PlaylistObject> PostPlaylistAsync(SpotifyToken token, string userId, PostPlaylist playlist);
        Task<bool> PostPlaylistTracksAsync(SpotifyToken token, string playlistId, PostPlaylistTracks tracks);

        Task<bool> IsFollowPlaylistAsync(SpotifyToken token, string playlistId);
        Task<bool> FollowPlaylistAsync(SpotifyToken token, string playlistId);
        Task<bool> PutTracksAsync(SpotifyToken token, PutTracks tracks);
    }
    public class SpotifyService : ISpotifyService
    {
        private readonly HttpClient _apiClient;
        private readonly SpotifySettings _spotifySettings;

        public SpotifyService(HttpClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _spotifySettings = configuration.GetSection(nameof(SpotifySettings)).Get<SpotifySettings>();
        }

        public string GetAuthorizationUrl(string state)
        {
            var queryData = new Dictionary<string, string>{
                {"client_id", _spotifySettings.ClientId},
                {"response_type", "code"},
                {"scope", _spotifySettings.Scope},
                {"redirect_uri", _spotifySettings.RedirectUri},
                {"state", state},
                {"show_dialog", "true"}
            };
            return QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize?", queryData);
        }

        public async Task<SpotifyToken> GetAuthorizationTokenAsync(string code)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://accounts.spotify.com")
            };

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _spotifySettings.RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            });
            var credentials = Encoding.ASCII.GetBytes($"{_spotifySettings.ClientId}:{_spotifySettings.ClientSecret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
            var response = await client.PostAsync("/api/token", form);
            return await response.JsonContentObject<SpotifyToken>();
        }

        public async Task<GetMe> GetMeAsync(SpotifyToken token)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync("me");
            return await response.JsonContentObject<GetMe>();
        }

        public async Task<GetTracks> GetTracksAsync(SpotifyToken token, int offset = 0, int limit = 50)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync($"me/tracks?offset={offset}&limit={limit}");
            return await response.JsonContentObject<GetTracks>();
        }

        public async Task<GetPlaylists> GetPlaylistsAsync(SpotifyToken token, int offset = 0, int limit = 50)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync($"me/playlists?offset={offset}&limit={limit}");
            return await response.JsonContentObject<GetPlaylists>();
        }

        public async Task<GetPlaylistsTracks> GetPlaylistsTracksAsync(SpotifyToken token, string playlistId, int offset = 0, int limit = 50)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync($"playlists/{playlistId}/tracks?offset={offset}&limit={limit}");
            return await response.JsonContentObject<GetPlaylistsTracks>();
        }

        public async Task<PlaylistObject> PostPlaylistAsync(SpotifyToken token, string userId, PostPlaylist playlist)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.PostAsJsonAsync($"users/{userId}/playlists", playlist);
            return await response.JsonContentObject<PlaylistObject>();
        }

        public async Task<bool> PostPlaylistTracksAsync(SpotifyToken token, string playlistId, PostPlaylistTracks tracks)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.PostAsJsonAsync($"playlists/{playlistId}/tracks", tracks);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutTracksAsync(SpotifyToken token, PutTracks tracks)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.PutAsync($"me/tracks?ids={tracks.Ids}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IsFollowPlaylistAsync(SpotifyToken token, string playlistId)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync($"playlists/{playlistId}/followers/contains");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> FollowPlaylistAsync(SpotifyToken token, string playlistId)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.PutAsync($"playlists/{playlistId}/followers", null);
            return response.IsSuccessStatusCode;
        }
    }

    public static class SpotifyServiceExtensions
    {
        public static IServiceCollection AddSpotifyApi(this IServiceCollection services)
        {
            services.AddTransient<ISpotifyService, SpotifyService>();
            services.AddHttpClient<ISpotifyService, SpotifyService>(client =>
            {
                client.BaseAddress = new Uri("https://api.spotify.com/v1/");
            });
            return services;
        }
    }
}