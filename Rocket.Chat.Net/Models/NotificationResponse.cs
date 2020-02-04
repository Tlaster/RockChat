using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class NotificationResponse
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public object Payload { get; set; }
    }
}