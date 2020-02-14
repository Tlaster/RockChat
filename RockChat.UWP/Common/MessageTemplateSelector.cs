using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RockChat.UWP.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    internal class MessageTemplateSelector : DataTemplateSelector, IItemsSourceSelector, IWithExtraDataSelector
    {
        private readonly DataTemplate _emptyTemplate = new DataTemplate();
        public bool IsInThread { get; set; }
        public DataTemplate MessageTemplate { get; set; }
        public DataTemplate LiteMessageTemplate { get; set; }
        public DataTemplate ThreadMessageTemplate { get; set; }
        public DataTemplate ThreadMessageLiteTemplate { get; set; }
        public DataTemplate ActionMessageTemplate { get; set; }
        public DataTemplate BlockMessageTemplate { get; set; }

        public object ItemsSource { get; set; }
        public object ExtraData { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is MessageData message && ItemsSource is IList<MessageData> items)
            {
                var index = items.IndexOf(message);
                var lastMessage = items.ElementAtOrDefault(index - 1);
                if (!string.IsNullOrEmpty(message.Type))
                {
                    return ActionMessageTemplate;
                }
                if (!string.IsNullOrEmpty(message.Tmid) && !IsInThread)
                {
                    if ((lastMessage?.Tmid != null && lastMessage.Tmid == message.Tmid) || (lastMessage?.Id == message.Tmid && message.Time - lastMessage?.Time <= TimeSpan.FromMinutes(1)))
                    {
                        return ThreadMessageLiteTemplate;
                    }
                    if (message.ThreadMessage == null)
                    {
                        var thread = items.FirstOrDefault(it => it.Id == message.Tmid);
                        if (thread != null)
                        {
                            message.ThreadMessage = thread;
                        }
                        else
                        {
                            //TODO: Request load message
                        }
                    }
                    return ThreadMessageTemplate;
                }
                if (ExtraData is RoomModel model && model.SubscriptionResult.Ignored?.Any(it => it == message.User.Id) == true)
                {
                    return BlockMessageTemplate;
                }

                if (lastMessage != null && message.Time - lastMessage.Time <= TimeSpan.FromMinutes(1) &&
                    lastMessage.Name == message.Name && lastMessage.Tmid == null)
                {
                    return LiteMessageTemplate;
                }

                return MessageTemplate;
            }

            return _emptyTemplate;
        }
    }
}