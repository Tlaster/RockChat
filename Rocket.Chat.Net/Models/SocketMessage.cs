using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class SocketMessage
    {
        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg { get; }

        public SocketMessage(string msg)
        {
            Msg = msg;
        }
    }
}