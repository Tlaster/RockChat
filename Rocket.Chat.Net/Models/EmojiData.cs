using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    
    public partial class CategoryData
    {
        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public long Order { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("category_label", NullValueHandling = NullValueHandling.Ignore)]
        public string CategoryLabel { get; set; }
    }

    public interface IEmojiData
    {
        string Name { get; }
        string Path { get; }
        string Category { get; }
        string Symbol { get; }
        bool Validate(string symbol);
    }

    public partial class RemoteEmojiData : IEmojiData
    {
        [JsonIgnore] public string Path => $"https://{Host}/emoji-custom/{Name}.{Extension}";
        [JsonIgnore] public string Category { get; } = "Custom";
        [JsonIgnore] public string Symbol => $":{Name}:";

        public bool Validate(string symbol)
        {
            var trim = symbol.Trim(':');
            return trim == Name || Aliases.Contains(trim);
        }

        [JsonIgnore] public string Host { get; internal set; }

        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("aliases", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Aliases { get; set; }

        [JsonProperty("extension", NullValueHandling = NullValueHandling.Ignore)]
        public string Extension { get; set; }

        [JsonProperty("_updatedAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public partial class EmojiData : IEmojiData
    {
        [JsonIgnore] public string Path => CodePoints.Base;
        [JsonIgnore] public string Symbol => Shortname;
        public bool Validate(string symbol)
        {
            return symbol == Shortname || ShortnameAlternates.Contains(symbol);
        }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("unicode_version", NullValueHandling = NullValueHandling.Ignore)]
        public double? UnicodeVersion { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public long? Order { get; set; }

        [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
        public long? Display { get; set; }

        [JsonProperty("shortname", NullValueHandling = NullValueHandling.Ignore)]
        public string Shortname { get; set; }

        [JsonProperty("shortname_alternates", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ShortnameAlternates { get; set; }

        [JsonProperty("ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Ascii { get; set; }

        [JsonProperty("humanform", NullValueHandling = NullValueHandling.Ignore)]
        public long? Humanform { get; set; }

        [JsonProperty("diversity_base", NullValueHandling = NullValueHandling.Ignore)]
        public long? DiversityBase { get; set; }

        [JsonProperty("diversity")]
        public string[] Diversity { get; set; }

        [JsonProperty("diversity_children", NullValueHandling = NullValueHandling.Ignore)]
        public string[] DiversityChildren { get; set; }

        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Gender { get; set; }

        [JsonProperty("gender_children", NullValueHandling = NullValueHandling.Ignore)]
        public string[] GenderChildren { get; set; }

        [JsonProperty("code_points", NullValueHandling = NullValueHandling.Ignore)]
        public CodePoints CodePoints { get; set; }

        [JsonProperty("keywords", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Keywords { get; set; }
    }

    public partial class CodePoints
    {
        [JsonProperty("base", NullValueHandling = NullValueHandling.Ignore)]
        public string Base { get; set; }

        [JsonProperty("fully_qualified", NullValueHandling = NullValueHandling.Ignore)]
        public string FullyQualified { get; set; }

        [JsonProperty("decimal", NullValueHandling = NullValueHandling.Ignore)]
        public string Decimal { get; set; }

        [JsonProperty("diversity_parent")]
        public string DiversityParent { get; set; }

        [JsonProperty("gender_parent")]
        public string GenderParent { get; set; }
    }
}
