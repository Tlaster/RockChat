using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class AuthService
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty("buttonLabelText", NullValueHandling = NullValueHandling.Ignore)]
        public string ButtonLabelText { get; set; }

        [JsonProperty("buttonColor", NullValueHandling = NullValueHandling.Ignore)]
        public string ButtonColor { get; set; }

        [JsonProperty("buttonLabelColor", NullValueHandling = NullValueHandling.Ignore)]
        public string ButtonLabelColor { get; set; }

        [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Custom { get; set; }
    }
}