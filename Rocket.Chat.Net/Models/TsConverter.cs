using System;
using Newtonsoft.Json;

namespace Rocket.Chat.Net.Models
{
    internal class TsConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Ts) || t == typeof(Ts?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    DateTimeOffset dt;
                    if (DateTimeOffset.TryParse(stringValue, out dt))
                    {
                        return new Ts { DateTime = dt };
                    }
                    break;
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<DateModel>(reader);
                    return new Ts { UpdatedAt = objectValue };
            }
            throw new Exception("Cannot unmarshal type Ts");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Ts)untypedValue;
            if (value.DateTime != null)
            {
                serializer.Serialize(writer, value.DateTime.Value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
            if (value.UpdatedAt != null)
            {
                serializer.Serialize(writer, value.UpdatedAt);
                return;
            }
            throw new Exception("Cannot marshal type Ts");
        }

        public static readonly TsConverter Singleton = new TsConverter();
    }
}