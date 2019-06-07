using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> JsonContentObject<T>(this HttpResponseMessage message)
        {
            var jsonData = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}