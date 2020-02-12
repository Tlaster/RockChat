using Newtonsoft.Json;

namespace Rocket.Chat.Net.Common
{
    internal static class Extensions
    {
        private static readonly JsonSerializerSettings _defaultJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, _defaultJsonSerializerSettings);
        }
    }
}