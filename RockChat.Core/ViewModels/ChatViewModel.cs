using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RockChat.Core.Collection;
using RockChat.Core.Common;
using RockChat.Core.Models;
using RockChat.Core.PlatformServices;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Models;

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
        private readonly INotification _notification;

        public ChatViewModel(Guid instanceId)
        {
            Instance = RockApp.Current.ActiveInstance[instanceId];
            _notification = this.Platform<INotification>();
            Init();
        }

        public InstanceModel Instance { get; }
        public Func<string, bool> RoomMessage { get; set; }
        public string Host => Instance.Host;
        public bool IsLoading { get; private set; }
        public ObservableCollection<RoomModel> Rooms => Instance.Client.Rooms;

        private async void Init()
        {
            IsLoading = true;
            await Instance.Client.Initialization();
            Instance.Client.Notification += ClientOnNotification;
            IsLoading = false;
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
            await Instance.Client.SendMessage(model.RoomsResult.Id, text, tmid);
        }

        public async Task SendFile(RoomModel model, FileInfo fileInfo, string fileName, string description, string tmid = null)
        {
            await Instance.Client.SendFileMessage(fileInfo, model.RoomsResult.Id, fileName, description, tmid);
        }
    }
}