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
                "rm" => RockLocalization.Message_removed,
                "uj" => RockLocalization.Has_joined_the_channel,
                "ut" => RockLocalization.Has_joined_the_conversation,
                "r" => string.Format(RockLocalization.Room_name_changed, msg, username),
                "message_pinned" => RockLocalization.Message_pinned,
                "jitsi_call_started" => string.Format(RockLocalization.Started_call, username),
                "ul" => RockLocalization.Has_left_the_channel,
                "ru" => string.Format(RockLocalization.User_removed_by, msg, username),
                "au" => string.Format(RockLocalization.User_added_by, msg, username),
                "user-muted" => string.Format(RockLocalization.User_muted_by, msg, username),
                "user-unmuted" => string.Format(RockLocalization.User_unmuted_by, msg, username),
                "subscription-role-added" => string.Format(RockLocalization.User_role_add, msg, role, username),
                "subscription-role-removed" => string.Format(RockLocalization.User_role_remove, msg, role, username),
                "room_changed_description" => string.Format(RockLocalization.Room_changed_description, msg, username),
                "room_changed_announcement" => string.Format(RockLocalization.Room_changed_announcement, msg, username),
                "room_changed_topic" => string.Format(RockLocalization.Room_changed_topic, msg, username),
                "room_changed_privacy" => string.Format(RockLocalization.Room_changed_privacy, msg, username),
                "message_snippeted" => RockLocalization.Created_snippet,
                "discussion-created" => string.Format(RockLocalization.Discussion_Created, username, msg),
                _ => string.Empty,
            };

            return message;
        }
    }
}
