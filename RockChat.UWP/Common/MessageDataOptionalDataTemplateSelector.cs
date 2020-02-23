using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    class MessageDataOptionalDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _emptyTemplate = new DataTemplate();
        public DataTemplate DataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item?.GetType() == typeof(MessageData))
            {
                return DataTemplate;
            }

            return _emptyTemplate;
        }
    }
}