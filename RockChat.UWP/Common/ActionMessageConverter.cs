using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
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

            var msg = data.Msg;
            var username = data.User.UserName;
            var role = data.Role;

            var message = data.Type switch
            {
                "rm" => "Message_removed",
                "uj" => "Has_joined_the_channel",
                "ut" => "Has_joined_the_conversation",
                "r" => string.Format("Room_name_changed", msg, username),
                "message_pinned" => "Message_pinned",
                "jitsi_call_started" => string.Format("Started_call", username),
                "ul" => "Has_left_the_channel",
                "ru" => string.Format("User_removed_by", msg, username),
                "au" => string.Format("User_added_by", msg, username),
                "user-muted" => string.Format("User_muted_by", msg, username),
                "user-unmuted" => string.Format("User_unmuted_by", msg, username),
                "subscription-role-added" => string.Format("User_role_add", msg, role, username),
                "subscription-role-removed" => string.Format("User_role_remove", msg, role, username),
                "room_changed_description" => string.Format("Room_changed_description", msg, username),
                "room_changed_announcement" => string.Format("Room_changed_announcement", msg, username),
                "room_changed_topic" => string.Format("Room_changed_topic", msg, username),
                "room_changed_privacy" => string.Format("Room_changed_privacy", msg, username),
                "message_snippeted" => "Created_snippet",
                _ => string.Empty,
            };

            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
