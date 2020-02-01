using System;
using System.Collections.Generic;
using System.Text;

namespace RockChat.Core.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly Guid _instanceId;

        public ChatViewModel(Guid instanceId)
        {
            _instanceId = instanceId;
        }
    }
}
