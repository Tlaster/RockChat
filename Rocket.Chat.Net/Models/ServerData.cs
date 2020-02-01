using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class ServerData
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }
    }
}
