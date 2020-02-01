using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Chat.Net.Models
{
    public partial class LoginResumeParam
    {
        [JsonProperty("resume", NullValueHandling = NullValueHandling.Ignore)]
        public string Resume { get; set; }
    }

    
    public partial class PublicSetting
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Value { get; set; }
    }

}