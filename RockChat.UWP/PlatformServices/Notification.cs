using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using RockChat.Core.PlatformServices;
using NotificationData = RockChat.Core.PlatformServices.NotificationData;

namespace RockChat.UWP.PlatformServices
{
    class Notification : INotification
    {
        public void Show(NotificationData data)
        {
            var toast = new ToastContentBuilder().SetToastScenario(ToastScenario.Default)
                .AddText(data.Title, AdaptiveTextStyle.Title)
                .AddText(data.Content);
            if (!string.IsNullOrEmpty(data.Image))
            {
                toast.AddAppLogoOverride(new Uri(data.Image));
            }

            if (!string.IsNullOrEmpty(data.Attribution))
            {
                toast.AddAttributionText(data.Attribution);
            }
            var result = toast.Content;
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(result.GetXml()));
        }
    }
}
