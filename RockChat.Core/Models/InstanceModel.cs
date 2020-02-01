using System;
using Newtonsoft.Json;
using Rocket.Chat.Net;

namespace RockChat.Core.Models
{
    internal enum IMType
    {
        RocketChat
    }

    internal class InstanceModel
    {
        public IMType ImType { get; set; }
        public string UserId { get; set; }
        public string Host { get; set; }
        public string Token { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime Expires { get; set; }

        [JsonIgnore]
        public RocketClient Client { get; set; }
    }
}