using Newtonsoft.Json;

namespace Rocket.Chat.Net.Common
{
    static class Extensions
    {
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}