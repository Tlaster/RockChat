using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using RockChat.Core.Collection;
using RockChat.Core.Models;
using Rocket.Chat.Net.Models;

namespace RockChat.Core.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        public bool IsLoading { get; set; }
        private readonly InstanceModel _instance;
        public ObservableCollection<RoomModel> Rooms => _instance.Client.Rooms;

        public ChatViewModel(Guid instanceId)
        {
            _instance = RockApp.Current.ActiveInstance[instanceId];
            Init();
        }

        private async void Init()
        {
            IsLoading = true;
            await _instance.Client.Initialization();
            IsLoading = false;
        }
    }
}
