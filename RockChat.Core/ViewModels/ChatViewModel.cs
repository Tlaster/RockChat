using System;
using System.Collections.Generic;
using System.Text;
using RockChat.Core.Models;

namespace RockChat.Core.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly InstanceModel _instance;

        public ChatViewModel(Guid instanceId)
        {
            _instance = RockApp.Current.ActiveInstance[instanceId];
            Init();
        }

        private async void Init()
        {

        }
    }
}
