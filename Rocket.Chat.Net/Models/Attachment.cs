using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class Attachment : IMessage
    {
        [JsonIgnore] public string Name => AuthorName;
        public string Avatar => AuthorIcon;
        public DateTime Time => Ts?.DateTime?.DateTime ?? Ts?.UpdatedAt?.ToDateTime() ?? default;

        [JsonProperty("ts", NullValueHandling = NullValueHandling.Ignore)]
        public Ts? Ts { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("title_link", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleLink { get; set; }

        [JsonProperty("title_link_download", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TitleLinkDownload { get; set; }

        [JsonProperty("image_dimensions", NullValueHandling = NullValueHandling.Ignore)]
        public ImageDimensions ImageDimensions { get; set; }

        [JsonProperty("image_preview", NullValueHandling = NullValueHandling.Ignore)]
        public string ImagePreview { get; set; }

        [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUrl { get; set; }

        [JsonProperty("image_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageType { get; set; }

        [JsonProperty("image_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? ImageSize { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("thumb_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ThumbUrl { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Fields { get; set; }

        [JsonProperty("author_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AuthorName { get; set; }

        [JsonProperty("author_icon", NullValueHandling = NullValueHandling.Ignore)]
        public string AuthorIcon { get; set; }

        [JsonProperty("message_link", NullValueHandling = NullValueHandling.Ignore)]
        public string MessageLink { get; set; }

        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Attachment> Attachments { get; set; }
        
        [JsonProperty("video_url", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoUrl { get; set; }

        [JsonProperty("video_type", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoType { get; set; }

        [JsonProperty("video_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? VideoSize { get; set; }
    }
}