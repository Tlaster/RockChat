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

    public class SubscriptionCallMessage<T> : SocketMessage, IAsyncSocketCall
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; }
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public List<T> Params { get; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public SubscriptionCallMessage(string name, params T[] @params) : base("sub")
        {
            Name = name;
            Params = @params.ToList();
        }
    }
}