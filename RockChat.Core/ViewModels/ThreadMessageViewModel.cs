using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using RockChat.Core.Models;
using Rocket.Chat.Net.Models;

namespace RockChat.Core.ViewModels
{
    public class ThreadMessageViewModel : ViewModelBase, IDisposable
    {
        private readonly InstanceModel _instance;

        public ThreadMessageViewModel(string tmid, RoomModel room, InstanceModel instance)
        {
            Tmid = tmid;
            Room = room;
            _instance = instance;
            Init();
        }

        public string Tmid { get; }
        public bool IsLoading { get; private set; }
        public RoomModel Room { get; }
        public ObservableCollection<MessageData> Message { get; } = new ObservableCollection<MessageData>();

        public void Dispose()
        {
            if (Room.Messages is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= RoomMessageOnCollectionChanged;
            }
        }

        private async void Init()
        {
            IsLoading = true;
            var result = await _instance.Client.GetThreadMessages(Tmid);
            result.ForEach(it => { Message.Add(it); });
            if (Room.Messages is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += RoomMessageOnCollectionChanged;
            }

            IsLoading = false;
        }

        private void RoomMessageOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems?.OfType<MessageData>()?.ToList() ?? new List<MessageData>();
            var oldItems = e.OldItems?.OfType<MessageData>()?.ToList() ?? new List<MessageData>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems.Any(it => it.Tmid == Tmid))
                    {
                        foreach (var item in newItems)
                        {
                            Message.Add(item);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // TODO: Replace message item
                    //if (newItems.Any(it => it.Tmid == _tmid && Message.Any(message => message.Id == it.Id)))
                    //{

                    //}
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}