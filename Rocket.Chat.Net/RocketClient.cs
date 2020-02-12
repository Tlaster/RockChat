using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public class RocketClient : IDisposable
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

        public bool Connected { get; private set; }

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

        //public async Task<List<RoomsResult>> BrowseChannels(string text, string workspace, string type, string sortBy,
        //    string sortDirection, int limit, long page)
        //{

        //}

        public async Task<List<MessageData>> GetThreadMessages(string tmid)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<object>(
                "getThreadMessages", new
                {
                    tmid
                }));
            return result.Result.OrderBy(it => it.Ts.ToDateTime()).ToList();
        }

        public async Task<List<MessageData>> GetThreadsList(string rid, int limit = 50, long skip = 0)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<object>(
                "getThreadsList", new
                {
                    rid,
                    limit,
                    skip
                }));
            return result.Result;
        }

        public async Task Typing(string roomId, string name, bool isTyping)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("stream-notify-room",
                $"{roomId}/typing", name, isTyping));
        }

        public async Task ReadMessages(string roomId)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("readMessages", roomId));
        }

        public async Task SendMessage(string roomId, string message, string? tmid = null)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("sendMessage", new
            {
                _id = Guid.NewGuid(),
                rid = roomId,
                msg = message,
                tmid
            }));
        }

        public async Task SendFileMessage(FileInfo file, string rid, string? fileName = null,
            string? description = null, string? tmid = null, CancellationToken token = default, IProgress<float>? progress = default)
        {
            const string store = "Uploads";
            var type = MimeTypesMap.GetMimeType(file.Name);
            var response = await SocketCall<MethodCallResponse<UFSCreateResponse>>(
                new MethodCallMessage<UFSCreateParameter>("ufsCreate", new UFSCreateParameter
                {
                    Description = description ?? string.Empty,
                    Name = fileName ?? file.Name,
                    Rid = rid,
                    Size = file.Length,
                    Store = store,
                    Type = type
                }));
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie",
                $"rc_uid={_currentAccount.Id}; rc_token={_currentAccount.Token}");
            using var fileStream = file.OpenRead();
            using var streamContent = new StreamContent(fileStream);
            try
            {
                await client.PostAsync(response.Result.Url,
                    new ProgressableStreamContent(streamContent,
                        (sent, total) => { progress?.Report(Convert.ToSingle(sent) / Convert.ToSingle(total)); }),
                    token);
            }
            catch (TaskCanceledException)
            {
            }

            if (token.IsCancellationRequested)
            {
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("ufsStop",
                    response.Result.FileId, store, response.Result.Token));
            }
            else
            {
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("ufsComplete",
                    response.Result.FileId, store, response.Result.Token));
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("sendFileMessage", rid,
                    store,
                    new
                    {
                        _id = response.Result.FileId,
                        type,
                        size = file.Length,
                        name = fileName ?? file.Name,
                        description = description ?? string.Empty,
                        url =
                            $"/ufs/GridFS:{store}/{response.Result.FileId}/{HttpUtility.UrlEncode(fileName ?? file.Name)}"
                    }, new
                    {
                        msg = "",
                        tmid
                    }));
            }
        }

        public async Task AddRoomSubscription(string roomId)
        {
            await AddSubscription("stream-room-messages", $"{roomId}", OnRoomMessage);
            await AddSubscription("stream-notify-room", $"{roomId}/deleteMessage", OnDeleteRoomMessage);
            await AddSubscription("stream-notify-room", $"{roomId}/deleteMessageBulk", OnDeleteRoomMessageBulk);
            await AddSubscription("stream-notify-room", $"{roomId}/typing", OnRoomTyping);
        }

        private void OnDeleteRoomMessageBulk(List<object> obj)
        {
        }

        private void OnRoomTyping(List<object> obj)
        {
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
                for (var i = 0; i < room.Messages.Count; i++)
                {
                    if (room.Messages[i].Id == message.Id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    room.Messages[index] = message;
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
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    Ping(); // TODO: check ping && add timeout
                }
            });
        }

        public async Task<Dictionary<string, JToken>> GetPublicSettings()
        {
            var result =
                await SocketCall<MethodCallResponse<List<PublicSetting>>>(
                    new MethodCallMessage<object>("public-settings/get"));
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
            var result = await SocketCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginResumeParam>(
                "login",
                new LoginResumeParam
                {
                    Resume = token
                }));
            _currentAccount = result.Result;
            return _currentAccount;
        }

        public async Task<LoginResult> Login(string user, string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var digest = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            var result = await SocketCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginParam>("login",
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
            _currentAccount = result.Result;
            return _currentAccount;
        }

        public async Task<List<RoomsResult>> GetRooms()
        {
            var result =
                await SocketCall<MethodCallResponse<List<RoomsResult>>>(new MethodCallMessage<object>("rooms/get"));
            return result.Result;
        }

        public async Task<List<SubscriptionResult>> GetSubscription()
        {
            var result =
                await SocketCall<MethodCallResponse<List<SubscriptionResult>>>(
                    new MethodCallMessage<object>("subscriptions/get"));
            return result.Result;
        }

        public async Task<HistoryResult> LoadHistory(string roomId, int count, DateTime lastRefresh,
            DateTime? since = null)
        {
            var result = await SocketCall<MethodCallResponse<HistoryResult>>(new MethodCallMessage<object?>(
                "loadHistory",
                roomId,
                since?.ToDateModel(),
                count,
                lastRefresh.ToDateModel()
            ));
            return result.Result;
        }

        public async Task<List<MessageData>> GetMessages(params string[] id)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<string>(
                "getMessages",
                id
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