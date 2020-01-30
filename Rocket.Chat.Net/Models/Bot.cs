using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class Bot
    {
        [JsonProperty("i", NullValueHandling = NullValueHandling.Ignore)]
        public string I { get; set; }
    }
}