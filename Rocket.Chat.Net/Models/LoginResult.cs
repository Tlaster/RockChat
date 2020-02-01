using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class LoginResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty("tokenExpires", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel TokenExpires { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}