using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class HistoryResult
    {
        [JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
        public List<MessageData> Messages { get; set; }

        [JsonProperty("unreadNotLoaded", NullValueHandling = NullValueHandling.Ignore)]
        public long UnreadNotLoaded { get; set; }

    }
}