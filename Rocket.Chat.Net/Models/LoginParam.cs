using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class LoginParam
    {
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public LoginPassword Password { get; set; }
    }
}