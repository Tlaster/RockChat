using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RockChat.Core.Collection;
using RockChat.Core.Common;
using RockChat.Core.Models;
using RockChat.Core.PlatformServices;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace RockChat.Core.ViewModels
{
    public class ChatMessageDataSource : IIncrementalSource<MessageData>
    {
        private readonly RocketClient _client;
        private readonly string _roomId;
        private DateTime? _since;

        public ChatMessageDataSource(RocketClient client, string roomId)
        {
            _client = client;
            _roomId = roomId;
        }

        public async Task<IEnumerable<MessageData>> GetPagedItemsAsync(int pageIndex, int pageSize,
            CancellationToken cancellationToken = default)
        {
            var result = await _client.LoadHistory(_roomId, pageSize, DateTime.UtcNow, _since);
            _since = result.Messages.LastOrDefault()?.Ts.ToDateTime();
            return result.Messages;
        }
    }

    public class ChatViewModel : ViewModelBase
    {
        private readonly INotification? _notification;
        private List<CategoryData> _categories;

        public ChatViewModel(InstanceModel instance)
        {
            Instance = instance;
            _notification = this.Platform<INotification>();
            Init();
        }

        public InstanceModel Instance { get; }
        public Func<string, bool>? RoomMessage { get; set; }
        public string Host => Instance.Host;
        public bool IsLoading { get; private set; }
        public bool IsOffline => !Instance.Client.Connected;
        public ObservableCollection<RoomModel> Rooms => Instance.Client.Rooms;
        public List<IEmojiData>? Emojis { get; private set; }
        [DependsOn(nameof(Emojis))]
        public IEnumerable<IGrouping<string, IEmojiData>>? EmojiGrouped => Emojis?.GroupBy(it => it.Category).OrderBy(it => _categories.Find(cat => cat.Category == it.Key)?.Order ?? 0);

        private async void Init()
        {
            IsLoading = true;
            await Instance.Client.Initialization();
            _categories = Instance.Client.GetCategories();
            Emojis = await Instance.Client.GetEmojis();
            Instance.Client.Notification += ClientOnNotification;
            Instance.Client.Close += ClientOnClose;
            Instance.Client.Error += ClientOnError;
            IsLoading = false;
        }

        private void ClientOnError(object sender, ErrorEventArgs e)
        {
            ReConnect();
        }

        private void ClientOnClose(object sender, CloseEventArgs e)
        {
            ReConnect();
        }

        private async Task ReConnect()
        {
            this.Platform<IDispatcher>().RunOnMainThread(() => OnPropertyChanged(nameof(IsOffline)));
            await Instance.Client.ReConnect();
            this.Platform<IDispatcher>().RunOnMainThread(() => OnPropertyChanged(nameof(IsOffline)));
        }

        private void ClientOnNotification(object sender, NotificationResponse e)
        {
            var data = new NotificationData
            {
                Title = e.Title,
                Content = e.Text,
                Attribution = Host
            };
            if (e.Payload is JObject jobj)
            {
                var payload = jobj.ToObject<NotificationPayload>();
                if (payload != null)
                {
                    if (RoomMessage?.Invoke(payload.Rid) != true)
                    {
                        Rooms.FirstOrDefault(it => it.RoomsResult.Id == payload.Rid)
                            ?.Let(it => it.SubscriptionResult.Alert = true);
                    }

                    data.Image = $"https://{Host}/avatar/{payload.Sender.Username}";
                }
            }

            _notification?.Show(data);
        }

        public async Task FetchRoomHistory(RoomModel item)
        {
            if (IsOffline)
            {
                return;
            }
            if (item.Messages == null)
            {
                item.Messages = CreateCollection(new ChatMessageDataSource(Instance.Client, item.RoomsResult.Id));
                await Instance.Client.AddRoomSubscription(item.RoomsResult.Id);
            }

            await Instance.Client.ReadMessages(item.RoomsResult.Id);
            item.SubscriptionResult.Alert = false;
        }

        protected virtual IncrementalLoadingCollection<ChatMessageDataSource, MessageData> CreateCollection(
            ChatMessageDataSource source)
        {
            return new IncrementalLoadingCollection<ChatMessageDataSource, MessageData>(source, true, 50);
        }

        public async Task SendText(RoomModel model, string text, string tmid = null)
        {
            if (IsOffline)
            {
                return;
            }
            await Instance.Client.SendMessage(model.RoomsResult.Id, text, tmid);
        }

        public async Task SendFile(RoomModel model, FileInfo fileInfo, string fileName, string description,
            string tmid = null)
        {
            if (IsOffline)
            {
                return;
            }
            await Instance.Client.SendFileMessage(fileInfo, model.RoomsResult.Id, fileName, description, tmid);
        }

        public async Task UpdateMessage(MessageData currentEditingMessage, string text)
        {
            if (IsOffline)
            {
                return;
            }
            await Instance.Client.UpdateMessage(currentEditingMessage.Id, currentEditingMessage.Rid, text);
        }

        public async Task SetReaction(MessageData messageData, string reaction)
        {
            await Instance.Client.SetReaction(messageData.Id, reaction);
        }
    }
}