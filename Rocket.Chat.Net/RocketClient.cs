using System;
using System.Collections.Concurrent;
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
    public class RocketClient
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
        }

        public string Host { get; }

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

        public async Task<LoginResponse> Login(string user, string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var digest = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            return await MethodCall<LoginResponse>(new LoginMessage
            {
                Params =
                {
                    new LoginParam
                    {
                        User = new LoginUser
                        {
                            UserName = user
                        },
                        Password = new LoginPassword
                        {
                            Digest = digest,
                            Algorithm = "sha-256"
                        }
                    }
                }
            });
        }
    }
}