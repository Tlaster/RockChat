using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using RockChat.Core.PlatformServices;
using NotificationData = RockChat.Core.PlatformServices.NotificationData;

namespace RockChat.UWP.PlatformServices
{
    class Notification : INotification
    {
        private static readonly Regex _invalidXmlChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXmlChars.Replace(text, "");
        }

        public void Show(NotificationData data)
        {
            var toast = new ToastContentBuilder().SetToastScenario(ToastScenario.Default)
                .AddText(RemoveInvalidXmlChars(data.Title), AdaptiveTextStyle.Title)
                .AddText(RemoveInvalidXmlChars(data.Content));
            if (!string.IsNullOrEmpty(data.Image))
            {
                toast.AddAppLogoOverride(new Uri(data.Image));
            }

            if (!string.IsNullOrEmpty(data.Attribution))
            {
                toast.AddAttributionText(RemoveInvalidXmlChars(data.Attribution));
            }
            var result = toast.Content;
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(result.GetXml()));
        }
    }
}
