using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Humanizer;

namespace RockChat.UWP.Common
{
    static class XamlHelper
    {
        public static bool NotNullOrEmpty(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static Visibility InvertBoolToVisibility(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        public static bool InvertBool(bool value)
        {
            return !value;
        }

        public static Visibility IsNotNullToVisibility(object any)
        {
            return any == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string HumanizeDateTime(DateTime date)
        {
            return (DateTime.UtcNow - date).TotalHours > 3 ? date.ToString("f") : date.Humanize();
        }
    }
}
