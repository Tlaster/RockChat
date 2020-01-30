using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public class MethodCallMessage<T> : SocketMessage, IMethodCall<T>, IAsyncSocketCall
    {
        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; }
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public List<T> Params { get; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public MethodCallMessage(string method, params T[] @params) : base("method")
        {
            Method = method;
            Params = @params.ToList();
        }
    }
}