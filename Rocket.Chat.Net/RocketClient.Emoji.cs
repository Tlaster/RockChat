using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Chat.Net.Models;

namespace Rocket.Chat.Net
{
    partial class RocketClient
    {
        public async Task<List<IEmojiData>> GetEmojis()
        {
            var assembly = typeof(RocketClient).GetTypeInfo().Assembly;
            using var resource = assembly.GetManifestResourceStream("Rocket.Chat.Net.Emoji.emoji.json");
            using var streamReader = new StreamReader(resource);
            using var reader = new JsonTextReader(streamReader);
            var emojis = new JsonSerializer().Deserialize<Dictionary<string, EmojiData>>(reader);
            var result = emojis.Values.OfType<IEmojiData>().ToList();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Token", _currentAccount.Token);
            client.DefaultRequestHeaders.Add("X-User-Id", _currentAccount.Id);
            var emojiResult = await client.GetStringAsync($"https://{Host}/api/v1/emoji-custom.list");
            var emojiJobj = JsonConvert.DeserializeObject<JObject>(emojiResult);
            var remoteEmoji = emojiJobj["emojis"]["update"].ToObject<List<RemoteEmojiData>>();
            remoteEmoji.ForEach(it => it.Host = Host);
            result.AddRange(remoteEmoji.OfType<IEmojiData>());
            return result;
        }

    }
}
