using System;
using System.Collections.Generic;
using System.Text;

namespace RockChat.Core.PlatformServices
{
    public interface INotification
    {
        void Show(NotificationData data);
    }


    public class NotificationData
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string Attribution { get; set; }
    }
}
