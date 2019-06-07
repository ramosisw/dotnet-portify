using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Models.Spotify.Tracks
{
    public class PutTracks
    {
        [JsonProperty("ids")]
        public string Ids { get; set; }
    }
}