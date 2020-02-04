using System;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public static class DateModelExtension
    {
        public static DateModel ToDateModel(this DateTime dateTime)
        {
            return new DateModel
            {
                Date = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds()
            };
        }
        public static DateTime ToDateTime(this DateModel data)
        {
            return data.Date == null ? DateTime.MinValue : DateTimeOffset.FromUnixTimeMilliseconds(data.Date.Value).UtcDateTime;
        }
    }
    public partial class DateModel
    {
        [JsonProperty("$date", NullValueHandling = NullValueHandling.Ignore)]
        public long? Date { get; set; }
    }
}