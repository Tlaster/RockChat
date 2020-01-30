using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class UrlData
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("ignoreParse", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IgnoreParse { get; set; }
    }
}