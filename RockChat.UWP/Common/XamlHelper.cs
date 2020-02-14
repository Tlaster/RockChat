using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Humanizer;
using RockChat.Core.Collection.Data;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    static class XamlHelper
    {
        public static bool NotNullOrEmpty(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static Visibility InvertBoolToVisibility(bool? value)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }
            return value.Value ? Visibility.Collapsed : Visibility.Visible;
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

        public static Visibility IsNotEmptyToVisibility(IEnumerable items)
        {
            if (items == null)
            {
                return Visibility.Collapsed;
            }
            return items.OfType<object>().Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public static FontWeight AlertFontStyle(SubscriptionResult? result)
        {
            if (result.DisableNotifications == true)
            {
                return FontWeights.Normal;
            }
            return result.Alert == true ? FontWeights.Bold : FontWeights.Normal;
        }

        public static string ThreadText(MessageData data)
        {
            if (data == null)
            {
                return string.Empty;
            }
            return string.IsNullOrEmpty(data.Text) ? data.Attachments?.FirstOrDefault()?.Title : data.Text;
        }

        public static Visibility LoadingVisibility(object any)
        {
            if (any is ISupportIncrementalLoading loading)
            {
                return loading.IsLoading ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }
    }
}
