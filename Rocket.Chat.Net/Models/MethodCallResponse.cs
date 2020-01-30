using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class MethodCallResponse<T> : SocketMessage, IAsyncSocketCall
    {
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }
        
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public MethodCallResponse() : base("result")
        {
        }

    }
}