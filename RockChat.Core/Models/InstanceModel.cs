using System;
using System.Net;
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
        public ProxySettings? ProxySettings { get; set; }

        [JsonIgnore]
        internal RocketClient Client { get; set; }
    }

    public class ProxySettings
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public WebProxy ToWebProxy()
        {
            return  new WebProxy(new Uri(Url))
            {
                Credentials = new NetworkCredential(UserName, Password)
            };
        }
    }

}