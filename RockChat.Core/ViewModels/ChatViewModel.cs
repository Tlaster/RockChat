using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RockChat.Core.Collection;
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
        private DateTime? _since = null;

        public ChatMessageDataSource(RocketClient client, string roomId)
        {
            _client = client;
            _roomId = roomId;
        }

        public async Task<IEnumerable<MessageData>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var result = await _client.LoadHistory(_roomId, pageSize, DateTime.UtcNow, _since);
            _since = result.Messages.LastOrDefault()?.Ts.ToDateTime();
            return result.Messages;
        }
    }

    public class ChatViewModel : ViewModelBase
    {
        public string Host => _instance.Host;
        public bool IsLoading { get; private set; }
        private readonly InstanceModel _instance;
        private readonly INotification _notification;
        public ObservableCollection<RoomModel> Rooms => _instance.Client.Rooms;

        public ChatViewModel(Guid instanceId)
        {
            _instance = RockApp.Current.ActiveInstance[instanceId];
            _notification = this.Platform<INotification>();
            Init();
        }

        private async void Init()
        {
            IsLoading = true;
            await _instance.Client.Initialization();
            _instance.Client.Notification += ClientOnNotification;
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
                    data.Image = $"https://{Host}/avatar/{payload.Sender.Username}";
                }
            }

            _notification?.Show(data);
        }

        public async Task FetchRoomHistory(RoomModel item)
        {
            if (item.Messages == null)
            {
                item.Messages = CreateCollection(new ChatMessageDataSource(_instance.Client, item.RoomsResult.Id));
                await _instance.Client.AddRoomSubscription(item.RoomsResult.Id);
            }

            await _instance.Client.ReadMessages(item.RoomsResult.Id);
        }

        protected virtual IncrementalLoadingCollection<ChatMessageDataSource, MessageData> CreateCollection(
            ChatMessageDataSource source)
        {
            return new IncrementalLoadingCollection<ChatMessageDataSource, MessageData>(source, inverted: true, itemsPerPage: 50);
        }

        public async Task SendText(RoomModel model, string text)
        {
            await _instance.Client.SendMessage(model.RoomsResult.Id, text);
        }

        public async Task SendFile(RoomModel model, FileInfo fileInfo, string fileName, string description)
        {
            await _instance.Client.SendFileMessage(fileInfo, model.RoomsResult.Id, fileName, description);
        }
    }
}
