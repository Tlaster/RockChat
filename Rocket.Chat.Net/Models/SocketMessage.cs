using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{


    interface IAsyncSocketCall
    {
        string Id { get; set; }
    }
    
    interface IMethodCall
    {
        string Method { get; }
    }
    
    public class SocketMessage
    {
        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg { get; }

        public SocketMessage(string msg)
        {
            Msg = msg;
        }
    }
    
    
    
    public partial class LoginResponse : SocketMessage, IAsyncSocketCall
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public LoginResult Result { get; set; }

        public LoginResponse() : base("result")
        {
        }
    }

    public partial class LoginResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty("tokenExpires", NullValueHandling = NullValueHandling.Ignore)]
        public TokenExpires TokenExpires { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public partial class TokenExpires
    {
        [JsonProperty("$date", NullValueHandling = NullValueHandling.Ignore)]
        public long? Date { get; set; }
    }

    
    public partial class LoginMessage : SocketMessage, IMethodCall, IAsyncSocketCall
    {
        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; } = "login";

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public List<LoginParam> Params { get; set; } = new List<LoginParam>();

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public LoginMessage() : base("method")
        {
        }
    }

    public partial class LoginParam
    {
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public LoginUser User { get; set; }

        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public LoginPassword Password { get; set; }
    }

    public partial class LoginPassword
    {
        [JsonProperty("digest", NullValueHandling = NullValueHandling.Ignore)]
        public string Digest { get; set; }

        [JsonProperty("algorithm", NullValueHandling = NullValueHandling.Ignore)]
        public string Algorithm { get; set; }
    }

    public partial class LoginUser
    {
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }
    }

}