using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class LoginPassword
    {
        [JsonProperty("digest", NullValueHandling = NullValueHandling.Ignore)]
        public string Digest { get; set; }

        [JsonProperty("algorithm", NullValueHandling = NullValueHandling.Ignore)]
        public string Algorithm { get; set; }
    }
}