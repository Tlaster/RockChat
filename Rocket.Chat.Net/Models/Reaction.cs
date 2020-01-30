using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class Reaction
    {
        [JsonProperty("usernames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Usernames { get; set; }
    }
}