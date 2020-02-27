using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Humanizer;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public interface IMessage
    {
        string Text { get; }
        string Name { get; }
        string Avatar { get; }
        DateTime Time { get; }
        List<Attachment> Attachments { get; }
    }
    
    public partial class NotificationPayload
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("rid", NullValueHandling = NullValueHandling.Ignore)]
        public string Rid { get; set; }

        [JsonProperty("sender", NullValueHandling = NullValueHandling.Ignore)]
        public Sender Sender { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public Message Message { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg { get; set; }
    }

    public partial class Sender
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }


    public partial class MessageData: IMessage, INotifyPropertyChanged
    {
        [JsonIgnore] public string Avatar => JsonAvatar ?? $"/avatar/{User.UserName}";
        [JsonIgnore] public DateTime Time => Ts.ToDateTime();
        [JsonIgnore] public string Text => Msg;
        [JsonIgnore] public string Name => Alias ?? User.UserName;
        [JsonIgnore] public MessageData ThreadMessage { get; set; }
        [JsonIgnore] public bool IsSelected { get; set; }

        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("rid", NullValueHandling = NullValueHandling.Ignore)]
        public string Rid { get; set; }

        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg { get; set; }

        [JsonProperty("ts", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel Ts { get; set; }

        [JsonProperty("u", NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        [JsonProperty("unread", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Unread { get; set; }

        [JsonProperty("_updatedAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateModel UpdatedAt { get; set; }

        [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
        public List<User> Mentions { get; set; }

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Channels { get; set; }

        [JsonProperty("urls", NullValueHandling = NullValueHandling.Ignore)]
        public List<UrlData> Urls { get; set; }

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public FileData File { get; set; }

        [JsonProperty("groupable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Groupable { get; set; }

        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Attachment> Attachments { get; set; }

        [JsonProperty("sandstormSessionId")]
        public object SandstormSessionId { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonAvatar { get; set; }

        [JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ReactionsConverter))]
        public Dictionary<string, Reaction> Reactions { get; set; }

        [JsonProperty("parseUrls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ParseUrls { get; set; }

        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public Bot Bot { get; set; }
        
        [JsonProperty("tmid", NullValueHandling = NullValueHandling.Ignore)]
        public string Tmid { get; set; }

        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }
        
        [JsonProperty("drid", NullValueHandling = NullValueHandling.Ignore)]
        public string Drid { get; set; }
        
        [JsonProperty("dcount", NullValueHandling = NullValueHandling.Ignore)]
        public long DCount { get; set; }

        [JsonProperty("tcount", NullValueHandling = NullValueHandling.Ignore)]
        public long TCount { get; set; }
        
        [JsonProperty("replies", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Replies { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    internal class ReactionsConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Dictionary<string, Reaction>);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return serializer.Deserialize<Dictionary<string, Reaction>>(reader);
                case JsonToken.StartArray:
                    return null;
            }
            throw new Exception("Cannot unmarshal type Reactions");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            serializer.Serialize(writer, untypedValue);
        }
    }
}