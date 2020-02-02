using System;
using System.Collections.Generic;
using System.Linq;
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

    public class SubscriptionCallResponse : SocketMessage
    {
        [JsonProperty("subs", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Subs { get; set; } = new List<string>();
        public SubscriptionCallResponse() : base("ready")
        {
        }
    }

    public static class SubscriptionCallResponseExtension
    {
        public static Guid? GetId(this SubscriptionCallResponse response)
        {
            if (Guid.TryParse(response.Subs.FirstOrDefault(), out var result))
            {
                return result;
            }

            return null;
        }
    }
}