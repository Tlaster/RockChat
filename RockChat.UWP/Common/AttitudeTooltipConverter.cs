using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    class AttitudeTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is KeyValuePair<string, Reaction> reaction))
            {
                return string.Empty;
            }

            return $"{string.Join(",", reaction.Value.Usernames)}: {reaction.Key}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
