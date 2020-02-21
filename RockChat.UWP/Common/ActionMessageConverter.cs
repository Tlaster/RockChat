using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using RockChat.Core.Common;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    class ActionMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is MessageData data))
            {
                return string.Empty;
            }

            return ActionMessageHelper.GetMessage(data);
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
