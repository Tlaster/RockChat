using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rocket.Chat.Net.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoomType
    {
        [EnumMember(Value = "c")]
        Chat,
        [EnumMember(Value = "d")]
        DirectChat,
        [EnumMember(Value = "p")]
        PrivateChat,
        [EnumMember(Value = "l")]
        LiveChat
    };
}