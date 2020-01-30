using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    public partial class ImageDimensions
    {
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }
    }
}