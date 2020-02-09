using System;
using Newtonsoft.Json;
using Rocket.Chat.Net;

namespace RockChat.Core.Models
{
    public enum IMType
    {
        RocketChat
    }

    public class InstanceModel
    {
        public IMType ImType { get; set; }
        public string UserId { get; set; }
        public string Host { get; set; }
        public string Token { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime Expires { get; set; }

        [JsonIgnore]
        internal RocketClient Client { get; set; }
    }
}