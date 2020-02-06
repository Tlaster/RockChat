using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace RockChat.UWP.Common
{
    class AvatarConverter : IValueConverter
    {
        public string Host { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string name))
            {
                return string.Empty;
            }

            return $"https://{Host}/avatar/{name}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class WithHostConverter : IValueConverter
    {
        public string Host { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string str))
            {
                return string.Empty;
            }

            if (targetType == typeof(Uri))
            {
                return new Uri($"https://{Host}{str}");
            }
            else
            {
                return $"https://{Host}{str}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
