using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

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
    }
}
