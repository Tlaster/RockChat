using System;
using System.Collections.Generic;
using System.Text;
using RockChat.Core.i18n;
using Rocket.Chat.Net.Models;

namespace RockChat.Core.Common
{
    public static class ActionMessageHelper
    {
        public static string GetMessage(MessageData data)
        {
            var msg = data.Msg;
            var username = data.User.UserName;
            var role = data.Role;

            var message = data.Type switch
            {
                "rm" => Localization.Message_removed,
                "uj" => Localization.Has_joined_the_channel,
                "ut" => Localization.Has_joined_the_conversation,
                "r" => string.Format(Localization.Room_name_changed, msg, username),
                "message_pinned" => Localization.Message_pinned,
                "jitsi_call_started" => string.Format(Localization.Started_call, username),
                "ul" => Localization.Has_left_the_channel,
                "ru" => string.Format(Localization.User_removed_by, msg, username),
                "au" => string.Format(Localization.User_added_by, msg, username),
                "user-muted" => string.Format(Localization.User_muted_by, msg, username),
                "user-unmuted" => string.Format(Localization.User_unmuted_by, msg, username),
                "subscription-role-added" => string.Format(Localization.User_role_add, msg, role, username),
                "subscription-role-removed" => string.Format(Localization.User_role_remove, msg, role, username),
                "room_changed_description" => string.Format(Localization.Room_changed_description, msg, username),
                "room_changed_announcement" => string.Format(Localization.Room_changed_announcement, msg, username),
                "room_changed_topic" => string.Format(Localization.Room_changed_topic, msg, username),
                "room_changed_privacy" => string.Format(Localization.Room_changed_privacy, msg, username),
                "message_snippeted" => Localization.Created_snippet,
                _ => string.Empty,
            };

            return message;
        }
    }
}
