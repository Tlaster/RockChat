using System;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class UrlData
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("ignoreParse", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IgnoreParse { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Meta Meta { get; set; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public Headers Headers { get; set; }

        [JsonProperty("parsedUrl", NullValueHandling = NullValueHandling.Ignore)]
        public ParsedUrl ParsedUrl { get; set; }
    }

    public partial class Headers
    {
        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty("contentLength", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentLength { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("pageTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string PageTitle { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("ogType", NullValueHandling = NullValueHandling.Ignore)]
        public string OgType { get; set; }

        [JsonProperty("ogTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string OgTitle { get; set; }

        [JsonProperty("ogUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string OgUrl { get; set; }

        [JsonProperty("ogSiteName", NullValueHandling = NullValueHandling.Ignore)]
        public string OgSiteName { get; set; }

        [JsonProperty("ogDescription", NullValueHandling = NullValueHandling.Ignore)]
        public string OgDescription { get; set; }

        [JsonProperty("ogLocale", NullValueHandling = NullValueHandling.Ignore)]
        public string OgLocale { get; set; }

        [JsonProperty("ogUpdatedTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? OgUpdatedTime { get; set; }

        [JsonProperty("twitterCard", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterCard { get; set; }

        [JsonProperty("twitterTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterTitle { get; set; }

        [JsonProperty("twitterDescription", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterDescription { get; set; }
    }

    public partial class ParsedUrl
    {
        [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
        public string Host { get; set; }

        [JsonProperty("hash")]
        public object Hash { get; set; }

        [JsonProperty("pathname", NullValueHandling = NullValueHandling.Ignore)]
        public string Pathname { get; set; }

        [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
        public string Protocol { get; set; }

        [JsonProperty("port")]
        public object Port { get; set; }

        [JsonProperty("query")]
        public object Query { get; set; }

        [JsonProperty("search")]
        public object Search { get; set; }

        [JsonProperty("hostname", NullValueHandling = NullValueHandling.Ignore)]
        public string Hostname { get; set; }
    }
}