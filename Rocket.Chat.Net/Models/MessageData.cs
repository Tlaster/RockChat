using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class MessageData
    {
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
        public string Avatar { get; set; }

        [JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Reaction> Reactions { get; set; }

        [JsonProperty("parseUrls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ParseUrls { get; set; }

        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public Bot Bot { get; set; }

        [JsonProperty("tmid", NullValueHandling = NullValueHandling.Ignore)]
        public string Tmid { get; set; }
    }
}