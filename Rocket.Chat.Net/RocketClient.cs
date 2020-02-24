using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HeyRed.Mime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace Rocket.Chat.Net
{
    public partial class RocketClient : IDisposable
    {
        private readonly AutoResetEvent _connectEvent = new AutoResetEvent(false);
        private readonly IDispatcher _dispatcher;
        private readonly List<RoomsResult> _rooms = new List<RoomsResult>();
        private readonly WebSocket _socket;

        private readonly ConcurrentDictionary<Guid, AutoResetEvent> _socketEvents =
            new ConcurrentDictionary<Guid, AutoResetEvent>();

        private readonly ConcurrentDictionary<Guid, JObject> _socketResult = new ConcurrentDictionary<Guid, JObject>();

        private readonly Dictionary<string, Action<List<object>>> _socketSubscriptions =
            new Dictionary<string, Action<List<object>>>();

        private readonly List<SubscriptionResult> _subscriptions = new List<SubscriptionResult>();

        private LoginResult _currentAccount;
        private IWebProxy? _proxy;
        private bool _connected;

        public RocketClient(string host, IDispatcher? dispatcher = null)
        {
            Host = host;
            _dispatcher = dispatcher;
            _socket = new WebSocket($"wss://{Host}/websocket");
            _socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            _socket.OnOpen += SocketOnOpen;
            _socket.OnMessage += SocketOnMessage;
            _socket.OnClose += SocketOnClose;
            _socket.OnError += SocketOnError;
        }

        public ObservableCollection<RoomModel> Rooms { get; } = new ObservableCollection<RoomModel>();

        public bool Connected
        {
            get => _connected;
            private set
            {
                _connected = value;
                SendPingLoop();
            }
        }
        
        public IWebProxy? Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
                if (value == null)
                {
                    _socket.SetProxy(null, null, null);
                }
                else
                {
                    var userName = string.Empty;
                    var password = string.Empty;
                    var url = value.GetProxy(new Uri($"wss://{Host}/websocket")).ToString();
                    if (value.Credentials is NetworkCredential credential)
                    {
                        userName = credential.UserName;
                        password = credential.Password;
                    }

                    _socket.SetProxy(url, userName, password);
                }
            }
        }

        public string Host { get; }

        public void Dispose()
        {
            _connectEvent.Dispose();
            ((IDisposable) _socket).Dispose();
        }

        public event EventHandler<CloseEventArgs>? Close;
        public event EventHandler<ErrorEventArgs>? Error;
        public event EventHandler<NotificationResponse>? Notification;

        private void SocketOnError(object sender, ErrorEventArgs e)
        {
            Connected = false;
            Error?.Invoke(this, e);
        }

        private void SocketOnClose(object sender, CloseEventArgs e)
        {
            Connected = false;
            Close?.Invoke(this, e);
        }

        public void Disconnect()
        {
            _socket.Close();
        }

        public async Task ReConnect()
        {
            _socketSubscriptions.Clear();
            await Connect();
            await Login(_currentAccount.Token);
            await Task.WhenAll(
                AddSubscription("stream-notify-logged", "Users:NameChanged", UserNameChangedHandler),
                AddSubscription("stream-notify-logged", "Users:Deleted", UserDeleteHandler),
                AddSubscription("stream-notify-logged", "deleteEmojiCustom", CustomEmojiHandler),
                AddSubscription("stream-notify-logged", "updateEmojiCustom", CustomEmojiHandler),
                AddSubscription("stream-notify-logged", "user-status", UserStatusHandler),
                AddSubscription("stream-notify-logged", "permissions-changed", PermissionChangedHandler),
                AddSubscription("stream-notify-logged", "roles-change", RolesChangeHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/message", UserMessageHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/userData", UserDataHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/notification", UserNotificationHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/rooms-changed", RoomsChangedHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/subscriptions-changed",
                    UserSubscriptionHandler),
                Task.Run(async () =>
                {
                    _rooms.Clear();
                    _rooms.AddRange(await GetRooms());
                }),
                Task.Run(async () =>
                {
                    _subscriptions.Clear();
                    _subscriptions.AddRange(await GetSubscription());
                }));
            var roomsToRemove = new List<RoomModel>();
            foreach (var room in Rooms)
            {
                room.RoomsResult = _rooms.FirstOrDefault(it => it.Id == room.RoomsResult.Id);
                room.SubscriptionResult = _subscriptions.FirstOrDefault(it => it.Rid == room.RoomsResult.Id);
                if (room.RoomsResult == null)
                {
                    roomsToRemove.Add(room);
                    continue;
                }
                if (room.Messages != null && room.RoomsResult != null)
                {
                    if (room.Messages.Any())
                    {
                        var missed = await LoadMissedMessages(room.RoomsResult.Id,
                            room.Messages.LastOrDefault()?.Time ?? DateTime.UtcNow);
                        missed.Reverse();
                        missed.ForEach(it => room.Messages.Add(it));
                    }
                    await AddRoomSubscription(room.RoomsResult.Id);
                }
            }
            roomsToRemove.ForEach(it => Rooms.Remove(it));
        }

        public Task Connect()
        {
            return Task.Run(() =>
            {
                _socket.Connect();
                _connectEvent.WaitOne();
            });
        }

        public async Task Initialization()
        {
            await Task.WhenAll(
                AddSubscription("stream-notify-logged", "Users:NameChanged", UserNameChangedHandler),
                AddSubscription("stream-notify-logged", "Users:Deleted", UserDeleteHandler),
                AddSubscription("stream-notify-logged", "deleteEmojiCustom", CustomEmojiHandler),
                AddSubscription("stream-notify-logged", "updateEmojiCustom", CustomEmojiHandler),
                AddSubscription("stream-notify-logged", "user-status", UserStatusHandler),
                AddSubscription("stream-notify-logged", "permissions-changed", PermissionChangedHandler),
                AddSubscription("stream-notify-logged", "roles-change", RolesChangeHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/message", UserMessageHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/userData", UserDataHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/notification", UserNotificationHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/rooms-changed", RoomsChangedHandler),
                AddSubscription("stream-notify-user", $"{_currentAccount.Id}/subscriptions-changed",
                    UserSubscriptionHandler),
                Task.Run(async () => _rooms.AddRange(await GetRooms())),
                Task.Run(async () => _subscriptions.AddRange(await GetSubscription()))
            );
            _rooms.Select(room =>
            {
                return new RoomModel
                {
                    Host = Host,
                    RoomsResult = room,
                    SubscriptionResult = _subscriptions.FirstOrDefault(it => it.Rid == room.Id)
                };
            }).ToList().ForEach(it => { Rooms.Add(it); });
            //await AddSubscription("stream-roles", $"roles", UserSubscriptionHandler);
            //await AddSubscription("stream-importers", $"progress", UserSubscriptionHandler);
        }


        public async Task AddRoomSubscription(string roomId)
        {
            await AddSubscription("stream-room-messages", $"{roomId}", OnRoomMessage);
            await AddSubscription("stream-notify-room", $"{roomId}/deleteMessage", OnDeleteRoomMessage);
            //await AddSubscription("stream-notify-room", $"{roomId}/deleteMessageBulk", OnDeleteRoomMessageBulk);
            await AddSubscription("stream-notify-room", $"{roomId}/typing", (args) => OnRoomTyping(roomId, args));
        }

        private void OnDeleteRoomMessageBulk(List<object> obj)
        {
        }

        private void OnRoomTyping(string roomId, List<object> args)
        {
            var name = args.FirstOrDefault().ToString();
            bool.TryParse(args.ElementAtOrDefault(1).ToString(), out var typing);
            var room = Rooms.FirstOrDefault(it => it.RoomsResult.Id == roomId);
            if (room != null)
            {
                _dispatcher.RunOnMainThread(() =>
                {
                    if (typing && !string.IsNullOrEmpty(name) && !room.Typing.Contains(name))
                    {
                        room.Typing.Add(name);
                    }
                    else
                    {
                        room.Typing.Remove(name);
                    }
                });
            }
        }

        private void OnDeleteRoomMessage(List<object> obj)
        {
        }

        private void OnRoomMessage(List<object> obj)
        {
            var jobj = obj.FirstOrDefault() as JObject;
            var message = jobj?.ToObject<MessageData>();
            if (message == null)
            {
                return;
            }

            var room = Rooms.FirstOrDefault(it => it.RoomsResult.Id == message.Rid);
            if (room?.Messages == null)
            {
                return;
            }

            _dispatcher?.RunOnMainThread(() =>
            {
                var index = -1;
                var tindex = -1;
                for (var i = 0; i < room.Messages.Count; i++)
                {
                    if (room.Messages[i].Id == message.Id)
                    {
                        index = i;
                        break;
                    }

                    if (room.Messages[i].Tmid == message.Id)
                    {
                        tindex = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    room.Messages[index] = message;
                }
                else if (tindex != -1)
                {
                    room.Messages[tindex].ThreadMessage = message;
                }
                else
                {
                    room.Messages.Add(message);
                }
            });
        }

        private void RolesChangeHandler(List<object> obj)
        {
        }

        private void PermissionChangedHandler(List<object> obj)
        {
        }

        private void RoomsChangedHandler(List<object> obj)
        {
            var type = obj.FirstOrDefault() as string;
            var jobj = obj.ElementAtOrDefault(1) as JObject;
            var room = jobj?.ToObject<RoomsResult>();
            if (string.IsNullOrEmpty(type) || room == null || !_rooms.Any())
            {
                return;
            }

            switch (type)
            {
                case "updated":
                    var index = _rooms.FindIndex(it => it.Id == room.Id);
                    _rooms[index] = room;
                    var item = Rooms.FirstOrDefault(it => it.SubscriptionResult.Rid == room.Id);
                    if (item != null)
                    {
                        _dispatcher?.RunOnMainThread(() => { item.RoomsResult = room; });
                    }

                    break;
            }
        }

        private void UserSubscriptionHandler(List<object> obj)
        {
            var type = obj.FirstOrDefault() as string;
            var jobj = obj.ElementAtOrDefault(1) as JObject;
            var subscription = jobj?.ToObject<SubscriptionResult>();
            if (string.IsNullOrEmpty(type) || subscription == null || !_subscriptions.Any())
            {
                return;
            }

            switch (type)
            {
                case "updated":
                    var index = _subscriptions.FindIndex(it => it.Id == subscription.Id);
                    _subscriptions[index] = subscription;
                    var item = Rooms.FirstOrDefault(it => it.SubscriptionResult.Id == subscription.Id);
                    if (item != null)
                    {
                        _dispatcher?.RunOnMainThread(() => { item.SubscriptionResult = subscription; });
                    }

                    break;
                case "inserted":
                    _subscriptions.Add(subscription);
                    Rooms.Add(new RoomModel
                    {
                        Host = Host,
                        SubscriptionResult = subscription
                    });
                    break;
            }
        }

        private void UserDataHandler(List<object> obj)
        {
        }

        private void UserNotificationHandler(List<object> obj)
        {
            var jobj = obj.FirstOrDefault() as JObject;
            var item = jobj?.ToObject<NotificationResponse>();
            if (item == null)
            {
                return;
            }

            Notification?.Invoke(this, item);
        }

        private void UserStatusHandler(List<object> obj)
        {
        }

        private void UserDeleteHandler(List<object> obj)
        {
        }

        private void CustomEmojiHandler(List<object> obj)
        {
        }

        private void UserMessageHandler(List<object> obj)
        {
        }

        private void UserNameChangedHandler(List<object> @params)
        {
            if (!(@params.FirstOrDefault() is JObject jObject))
            {
                return;
            }

            var user = jObject.ToObject<User>();
            //TODO: update user information
        }

        private async Task AddSubscription(string name, string @event, Action<List<object>> handler)
        {
            var subscription =
                await SocketCall<SubscriptionCallResponse>(
                    new SubscriptionCallMessage<object>(name, @event, false));
            _socketSubscriptions.Add($"{name}:{@event}", handler);
        }

        private void SocketOnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                Debug.WriteLine($"socket receive from {Host}: {e.Data}");
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
                    Connected = true;
                    _connectEvent.Set();
                    break;
                case "ping":
                    Pong();
                    break;
                case "pong":
                    // Ping();
                    break;
                case "result":
                    ProcessSocketCallResult(message);
                    break;
                case "ready":
                    ProcessSocketSubscriptionResult(message);
                    break;
                case "updated":
                    break;
                case "changed":
                    ProcessSubscriptionResult(message);
                    break;
            }
        }

        private void ProcessSocketSubscriptionResult(JObject message)
        {
            var id = message.ToObject<SubscriptionCallResponse>()?.GetId();
            if (id != null)
            {
                if (_socketEvents.ContainsKey(id.Value))
                {
                    _socketResult.TryAdd(id.Value, message);
                    _socketEvents[id.Value].Set();
                }
            }
        }

        private void ProcessSubscriptionResult(JObject message)
        {
            var collection = message.Value<string>("collection");
            var eventName = message["fields"]?["eventName"]?.Value<string>();
            var args = message["fields"]?["args"]?.ToObject<List<object>>() ?? new List<object>();
            if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (_socketSubscriptions.TryGetValue($"{collection}:{eventName}", out var handler))
            {
                handler.Invoke(args);
            }
        }

        private void ProcessSocketCallResult(JObject message)
        {
            var id = Guid.Parse(message.Value<string>("id"));
            if (_socketEvents.ContainsKey(id))
            {
                _socketResult.TryAdd(id, message);
                _socketEvents[id].Set();
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

        private Task<T> SocketCall<T>(IAsyncSocketCall socketCall)
        {
            return Task.Run(() =>
            {
                var id = Guid.NewGuid();
                socketCall.Id = id.ToString();
                using var autoResetEvent = new AutoResetEvent(false);
                _socketEvents.TryAdd(id, autoResetEvent);
                var data = socketCall.ToJson();
                Debug.WriteLine($"socket send to {Host}: {data}");
                _socket.Send(data);
                autoResetEvent.WaitOne();
                _socketResult.TryGetValue(id, out var result);
                _socketEvents.TryRemove(id, out _);
                _socketResult.TryRemove(id, out _);
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
        }

        private HttpClient CreateHttpClient()
        {
            return new HttpClient(new HttpClientHandler
            {
                Proxy = this.Proxy
            });
        }
        private void SendPingLoop()
        {
            Task.Run(async () =>
            {
                while (Connected)
                {
                    Ping(); // TODO: check ping && add timeout
                    await Task.Delay(TimeSpan.FromSeconds(20));
                }
            });
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