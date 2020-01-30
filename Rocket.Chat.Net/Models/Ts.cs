using System;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    [JsonConverter(typeof(TsConverter))]
    public partial struct Ts
    {
        public DateTimeOffset? DateTime;
        public DateModel UpdatedAt;

        public static implicit operator Ts(DateTimeOffset DateTime) => new Ts { DateTime = DateTime };
        public static implicit operator Ts(DateModel UpdatedAt) => new Ts { UpdatedAt = UpdatedAt };
    }
}