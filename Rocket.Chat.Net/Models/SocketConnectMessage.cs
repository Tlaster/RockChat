using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class SocketConnectMessage : SocketMessage
    {
        public SocketConnectMessage(string msg, string version, params string[] support) : base(msg)
        {
            Version = version;
            Support = support.ToList();
        }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; }

        [JsonProperty("support", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Support { get; }
    }
}