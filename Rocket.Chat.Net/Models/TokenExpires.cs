using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class TokenExpires
    {
        [JsonProperty("$date", NullValueHandling = NullValueHandling.Ignore)]
        public long? Date { get; set; }
    }
}