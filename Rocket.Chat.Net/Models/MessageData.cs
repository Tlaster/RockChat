using System;
using System.Collections.Generic;
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

    public partial class MessageData: IMessage
    {
        [JsonIgnore] public string Avatar => JsonAvatar ?? $"/avatar/{User.UserName}";
        [JsonIgnore] public DateTime Time => Ts.ToDateTime();
        [JsonIgnore] public string Text => Msg;
        [JsonIgnore] public string Name => User.Name ?? User.UserName;

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
        public Reactions Reactions { get; set; }

        [JsonProperty("parseUrls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ParseUrls { get; set; }

        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public Bot Bot { get; set; }
        
        [JsonProperty("tmid", NullValueHandling = NullValueHandling.Ignore)]
        public string Tmid { get; set; }

        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    [JsonConverter(typeof(ReactionsConverter))]
    public partial struct Reactions
    {
        public List<object> AnythingArray;
        public Dictionary<string, Reaction> ReactionMap;

        public static implicit operator Reactions(List<object> AnythingArray) => new Reactions { AnythingArray = AnythingArray };
        public static implicit operator Reactions(Dictionary<string, Reaction> ReactionMap) => new Reactions { ReactionMap = ReactionMap };
    }
    
    internal class ReactionsConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Reactions) || t == typeof(Reactions?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Dictionary<string, Reaction>>(reader);
                    return new Reactions { ReactionMap = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<List<object>>(reader);
                    return new Reactions { AnythingArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Reactions");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Reactions)untypedValue;
            if (value.AnythingArray != null)
            {
                serializer.Serialize(writer, value.AnythingArray);
                return;
            }
            if (value.ReactionMap != null)
            {
                serializer.Serialize(writer, value.ReactionMap);
                return;
            }
            throw new Exception("Cannot marshal type Reactions");
        }
    }
}