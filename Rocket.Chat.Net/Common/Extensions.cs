using Newtonsoft.Json;

namespace Rocket.Chat.Net.Common
{
    internal static class Extensions
    {
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}