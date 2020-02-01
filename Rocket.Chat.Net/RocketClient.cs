using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;
using WebSocketSharp;

namespace Rocket.Chat.Net
{
    public class RocketClient : IDisposable
    {
        private readonly AutoResetEvent _connectEvent = new AutoResetEvent(false);

        private readonly ConcurrentDictionary<Guid, AutoResetEvent> _events =
            new ConcurrentDictionary<Guid, AutoResetEvent>();

        private readonly ConcurrentDictionary<Guid, JObject> _methodResult = new ConcurrentDictionary<Guid, JObject>();

        private readonly WebSocket _socket;


        public RocketClient(string host)
        {
            Host = host;
            _socket = new WebSocket($"wss://{Host}/websocket");
            _socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            _socket.OnOpen += SocketOnOpen;
            _socket.OnMessage += SocketOnMessage;
            _socket.OnClose += SocketOnClose;
            _socket.OnError += SocketOnError;
        }

        public string Host { get; }

        public void Dispose()
        {
            _connectEvent.Dispose();
            ((IDisposable) _socket).Dispose();
        }


        public event EventHandler<CloseEventArgs>? Close;
        public event EventHandler<ErrorEventArgs>? Error;

        private void SocketOnError(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        private void SocketOnClose(object sender, CloseEventArgs e)
        {
            Close?.Invoke(this, e);
        }

        public Task Connect()
        {
            return Task.Run(() =>
            {
                _socket.Connect();
                _connectEvent.WaitOne();
            });
        }

        private void SocketOnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                var message = JsonConvert.DeserializeObject<JObject>(e.Data);
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(JObject message)
        {
            var msg = message.Value<string>("msg");
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }

            switch (msg)
            {
                case "connected":
                    _connectEvent.Set();
                    break;
                case "ping":
                    Pong();
                    break;
                case "pong":
                    // Ping();
                    break;
                case "result":
                    ProcessMethodCallResult(message);
                    break;
                case "ready":
                    break;
                case "updated":
                    break;
                case "changed":
                    break;
            }
        }

        private void ProcessMethodCallResult(JObject message)
        {
            var id = Guid.Parse(message.Value<string>("id"));
            if (_events.ContainsKey(id))
            {
                _methodResult.TryAdd(id, message);
                _events[id].Set();
            }
        }

        private void Pong()
        {
            _socket.Send(new SocketMessage("pong").ToJson());
        }

        private void Ping()
        {
            _socket.Send(new SocketMessage("ping").ToJson());
        }

        private Task<T> MethodCall<T>(IAsyncSocketCall methodCall)
        {
            return Task.Run(() =>
            {
                var id = Guid.NewGuid();
                methodCall.Id = id.ToString();
                using var autoResetEvent = new AutoResetEvent(false);
                _events.TryAdd(id, autoResetEvent);
                _socket.Send(methodCall.ToJson());
                autoResetEvent.WaitOne();
                _methodResult.TryGetValue(id, out var result);
                _events.TryRemove(id, out _);
                _methodResult.TryRemove(id, out _);
                if (result != null && result.ContainsKey("error"))
                {
                    throw new RocketClientException(result["error"]?["message"]?.Value<string>() ?? string.Empty);
                }
                return result == null ? default : result.ToObject<T>();
            });
        }

        private void SocketOnOpen(object sender, EventArgs e)
        {
            _socket.Send(new SocketConnectMessage("connect", "1", "1").ToJson());
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    Ping();
                }
            });
        }

        public async Task<Dictionary<string, JToken>> GetPublicSettings()
        {
            var result =
                await MethodCall<MethodCallResponse<List<PublicSetting>>>(new MethodCallMessage<object>("public-settings/get"));
            return result.Result.ToDictionary(it => it.Id, it => it.Value);
        }

        public async Task<ServerData> GetServerInformation()
        {
            using var client = new HttpClient();
            var data = await client.GetStringAsync($"https://{Host}/api/info");
            return JsonConvert.DeserializeObject<ServerData>(data);
        }

        public async Task<LoginResult> Login(string token)
        {
            var result = await MethodCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginResumeParam>("login",
                new LoginResumeParam
                {
                    Resume = token
                }));
            return result.Result;
        }

        public async Task<LoginResult> Login(string user, string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var digest = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            var result = await MethodCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginParam>("login",
                new LoginParam
                {
                    User = new User
                    {
                        UserName = user
                    },
                    Password = new LoginPassword
                    {
                        Digest = digest,
                        Algorithm = "sha-256"
                    }
                }));
            return result.Result;
        }

        public async Task<List<RoomsResult>> GetRooms()
        {
            var result = await MethodCall<MethodCallResponse<List<RoomsResult>>>(new MethodCallMessage<object>("rooms/get"));
            return result.Result;
        }

        public async Task<List<SubscriptionResult>> GetSubscriptions()
        {
            var result =
                await MethodCall<MethodCallResponse<List<SubscriptionResult>>>(
                    new MethodCallMessage<object>("subscriptions/get"));
            return result.Result;
        }

        public async Task<HistoryResult> LoadHistory(string roomId, int count, DateTime lastRefresh, DateTime? since = null)
        {
            var result = await MethodCall<MethodCallResponse<HistoryResult>>(new MethodCallMessage<object?>(
                "loadHistory",
                roomId,
                since?.ToDateModel(),
                count,
                lastRefresh.ToDateModel()
            ));
            return result.Result;
        }
    }

    [Serializable]
    public class RocketClientException : Exception
    {
        public RocketClientException()
        {
        }

        public RocketClientException(string message) : base(message)
        {
        }

        public RocketClientException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RocketClientException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}