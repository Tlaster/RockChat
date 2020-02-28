using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace Rocket.Chat.Net.Models
{
    public class RoomModel : INotifyPropertyChanged
    {
        public RoomsResult RoomsResult { get; set; }
        public SubscriptionResult SubscriptionResult { get; set; }
        public string Host { get; set; }
        [DependsOn(nameof(SubscriptionResult))]
        public string Name => SubscriptionResult.Name ?? SubscriptionResult.Fname;
        [DependsOn(nameof(RoomsResult), nameof(SubscriptionResult))]
        public string Avatar => $"https://{Host}/avatar/{RoomsResult.Topic ?? SubscriptionResult.Name}";
        [DependsOn(nameof(RoomsResult))]
        public DateTime UpdateAt => RoomsResult?.UpdatedAt?.ToDateTime() ?? default;
        public IList<MessageData>? Messages { get; set; }
        public ObservableCollection<string> Typing { get; } = new ObservableCollection<string>();
        public long Unread { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    

    public partial class RoomsResult : INotifyPropertyChanged
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("fname", NullValueHandling = NullValueHandling.Ignore)]
        public string Fname { get; set; }

        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public RoomType? RoomType { get; set; }

        [JsonProperty("usersCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? UsersCount { get; set; }

        [JsonProperty("u", NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        [JsonProperty("customFields", NullValueHandling = NullValueHandling.Ignore)]
        public JObject CustomFields { get; set; }

        [JsonProperty("broadcast", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Broadcast { get; set; }

        [JsonProperty("encrypted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Encrypted { get; set; }

        [JsonProperty("ro", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReadOnly { get; set; }

        [JsonProperty("sysMes", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SysMes { get; set; }

        [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Default { get; set; }

        [JsonProperty("_updatedAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel UpdatedAt { get; set; }

        [JsonProperty("lastMessage", NullValueHandling = NullValueHandling.Ignore)]
        public MessageData LastMessage { get; set; }

        [JsonProperty("e2eKeyId", NullValueHandling = NullValueHandling.Ignore)]
        public string E2EKeyId { get; set; }

        [JsonProperty("jitsiTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel JitsiTimeout { get; set; }

        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("prid", NullValueHandling = NullValueHandling.Ignore)]
        public string Prid { get; set; }

        [JsonProperty("announcement", NullValueHandling = NullValueHandling.Ignore)]
        public string Announcement { get; set; }

        [JsonProperty("reactWhenReadOnly", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReactWhenReadOnly { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class SubscriptionResult : INotifyPropertyChanged
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("open", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Open { get; set; }

        [JsonProperty("alert", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Alert { get; set; }

        [JsonProperty("unread", NullValueHandling = NullValueHandling.Ignore)]
        public long? Unread { get; set; }

        [JsonProperty("userMentions", NullValueHandling = NullValueHandling.Ignore)]
        public long? UserMentions { get; set; }

        [JsonProperty("groupMentions", NullValueHandling = NullValueHandling.Ignore)]
        public long? GroupMentions { get; set; }

        [JsonProperty("ts", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel Ts { get; set; }

        [JsonProperty("rid", NullValueHandling = NullValueHandling.Ignore)]
        public string Rid { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("fname", NullValueHandling = NullValueHandling.Ignore)]
        public string Fname { get; set; }

        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public RoomType? RoomType { get; set; }

        [JsonProperty("u", NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        [JsonProperty("emailNotifications", NullValueHandling = NullValueHandling.Ignore)]
        public string EmailNotifications { get; set; }

        [JsonProperty("_updatedAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel UpdatedAt { get; set; }

        [JsonProperty("ls", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel Ls { get; set; }

        [JsonProperty("desktopNotifications", NullValueHandling = NullValueHandling.Ignore)]
        public string DesktopNotifications { get; set; }

        [JsonProperty("prid", NullValueHandling = NullValueHandling.Ignore)]
        public string Prid { get; set; }

        [JsonProperty("E2EKey", NullValueHandling = NullValueHandling.Ignore)]
        public string E2EKey { get; set; }

        [JsonProperty("ignored", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Ignored { get; set; }

        [JsonProperty("disableNotifications", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DisableNotifications { get; set; }

        [JsonProperty("hideUnreadStatus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideUnreadStatus { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Roles { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}