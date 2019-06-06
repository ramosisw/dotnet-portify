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

namespace Core.Services
{
    public interface ISpotifyService
    {
        string GetAuthorizationUrl(string state);
        Task<SpotifyToken> GetAuthorizationTokenAsync(string code);
        Task<MeMessage> GetMeAsync(SpotifyToken token);
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
                {"state", state}
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
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SpotifyToken>(jsonString);
        }

        public async Task<MeMessage> GetMeAsync(SpotifyToken token)
        {
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _apiClient.GetAsync("me");
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MeMessage>(jsonString);
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