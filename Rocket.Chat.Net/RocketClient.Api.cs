using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HeyRed.Mime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;

namespace Rocket.Chat.Net
{
    partial class RocketClient
    {
        public async Task<bool> JoinRoom(string rid, string? joinCode = null)
        {
            var result =
                await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string?>("joinRoom", rid, joinCode));
            return result.Result;
        }

        public async Task LeaveRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("leaveRoom", rid));
        }
        
        public async Task DeleteRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("eraseRoom", rid));
        }
        
        public async Task ArchiveRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("archiveRoom", rid));
        }
        
        public async Task UnArchiveRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("unarchiveRoom", rid));
        }
        
        public async Task HideRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("hideRoom", rid));
        }

        public async Task OpenRoom(string rid)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<string>("openRoom", rid));
        }

        public async Task ToggleFavorite(string rid, bool favorite)
        {
            await SocketCall<MethodCallResponse<bool>>(new MethodCallMessage<object>("toggleFavorite", rid, favorite));
        }

        public async Task<List<AuthService>> SettingsOAuth()
        {
            using var client = CreateHttpClient();
            var result = await client.GetStringAsync($"https://{Host}/api/v1/settings.oauth");
            var jobj = JsonConvert.DeserializeObject<JObject>(result);
            if (jobj["services"] is JArray array)
            {
                return array.ToObject<List<AuthService>>() ?? new List<AuthService>();
            }

            return new List<AuthService>();
        }

        //public async Task<List<RoomsResult>> BrowseChannels(string text, string workspace, string type, string sortBy,
        //    string sortDirection, int limit, long page)
        //{

        //}

        public async Task<List<MessageData>> GetThreadMessages(string tmid)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<object>(
                "getThreadMessages", new
                {
                    tmid
                }));
            return result.Result.OrderBy(it => it.Ts.ToDateTime()).ToList();
        }

        public async Task<List<MessageData>> GetThreadsList(string rid, int limit = 50, long skip = 0)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<object>(
                "getThreadsList", new
                {
                    rid,
                    limit,
                    skip
                }));
            return result.Result;
        }

        public async Task Typing(string roomId, string name, bool isTyping)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("stream-notify-room",
                $"{roomId}/typing", name, isTyping));
        }

        public async Task ReportMessage(string messageId, string reason)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<string>("reportMessage", messageId,
                reason));
        }

        public async Task DeleteMessage(string messageId)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<string>("deleteMessage", messageId));
        }

        public async Task SetReaction(string messageId, string reaction)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<string>("setReaction", reaction, messageId));
        }

        public async Task ReadMessages(string roomId)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("readMessages", roomId));
        }
        
        public async Task SendMessage(string roomId, string message, string? tmid = null)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("sendMessage", new
            {
                _id = Guid.NewGuid(),
                rid = roomId,
                msg = message,
                tmid
            }));
        }

        public async Task UpdateMessage(string id, string roomId, string message)
        {
            await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("updateMessage", new
            {
                _id = id,
                rid = roomId,
                msg = message,
            }));
        }

        public async Task SendFileMessage(FileInfo file, string rid, string? fileName = null,
            string? description = null, string? tmid = null, CancellationToken token = default, IProgress<float>? progress = default)
        {
            const string store = "Uploads";
            var type = MimeTypesMap.GetMimeType(file.Name);
            var response = await SocketCall<MethodCallResponse<UFSCreateResponse>>(
                new MethodCallMessage<UFSCreateParameter>("ufsCreate", new UFSCreateParameter
                {
                    Description = description ?? string.Empty,
                    Name = fileName ?? file.Name,
                    Rid = rid,
                    Size = file.Length,
                    Store = store,
                    Type = type
                }));
            using var client = CreateHttpClient();
            client.DefaultRequestHeaders.Add("Cookie",
                $"rc_uid={_currentAccount.Id}; rc_token={_currentAccount.Token}");
            using var fileStream = file.OpenRead();
            using var streamContent = new StreamContent(fileStream);
            try
            {
                await client.PostAsync(response.Result.Url,
                    new ProgressableStreamContent(streamContent,
                        (sent, total) => { progress?.Report(Convert.ToSingle(sent) / Convert.ToSingle(total)); }),
                    token);
            }
            catch (TaskCanceledException)
            {
            }

            if (token.IsCancellationRequested)
            {
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("ufsStop",
                    response.Result.FileId, store, response.Result.Token));
            }
            else
            {
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("ufsComplete",
                    response.Result.FileId, store, response.Result.Token));
                await SocketCall<MethodCallResponse<object>>(new MethodCallMessage<object>("sendFileMessage", rid,
                    store,
                    new
                    {
                        _id = response.Result.FileId,
                        type,
                        size = file.Length,
                        name = fileName ?? file.Name,
                        description = description ?? string.Empty,
                        url =
                            $"/ufs/GridFS:{store}/{response.Result.FileId}/{HttpUtility.UrlEncode(fileName ?? file.Name)}"
                    }, new
                    {
                        msg = "",
                        tmid
                    }));
            }
        }

        public async Task<Dictionary<string, JToken>> GetPublicSettings()
        {
            var result =
                await SocketCall<MethodCallResponse<List<PublicSetting>>>(
                    new MethodCallMessage<object>("public-settings/get"));
            return result.Result.ToDictionary(it => it.Id, it => it.Value);
        }

        public async Task<string> GetUsernameSuggestion()
        {
            var result = await SocketCall<MethodCallResponse<string>>(new MethodCallMessage<object>("getUsernameSuggestion"));
            return result.Result;
        }

        public async Task<ServerData> GetServerInformation()
        {
            using var client = CreateHttpClient();
            var data = await client.GetStringAsync($"https://{Host}/api/info");
            return JsonConvert.DeserializeObject<ServerData>(data);
        }

        public async Task<LoginResult> Login(string token)
        {
            var result = await SocketCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginResumeParam>(
                "login",
                new LoginResumeParam
                {
                    Resume = token
                }));
            _currentAccount = result.Result;
            return _currentAccount;
        }

        public async Task<User> GetUserInfo(string id)
        {
            using var client = CreateHttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Token", _currentAccount.Token);
            client.DefaultRequestHeaders.Add("X-User-Id", _currentAccount.Id);
            var data = await client.GetStringAsync($"https://{Host}/api/v1/users.info?userId={id}");
            var obj = JsonConvert.DeserializeObject<JObject>(data);
            return obj["user"].ToObject<User>();
        }

        public async Task<string> SetUsername(string name)
        {
            var result = await SocketCall<MethodCallResponse<string>>(new MethodCallMessage<object>("setUsername", name));
            return result.Result;
        }

        public async Task<LoginResult> OAuthLogin(string credentialToken, string credentialSecret)
        {
            var result = await SocketCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<object>("login",
                new 
                {
                    oauth = new
                    {
                        credentialToken,
                        credentialSecret
                    }
                }));
            _currentAccount = result.Result;
            return _currentAccount;
        }


        public async Task<LoginResult> Login(string user, string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var digest = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            var result = await SocketCall<MethodCallResponse<LoginResult>>(new MethodCallMessage<LoginParam>("login",
                new LoginParam
                {
                    User = new User
                    {
                        UserName = user
                    },
                    Password = new LoginPassword
                    {
                        Digest = digest,
                        Algorithm = "sha-256"
                    }
                }));
            _currentAccount = result.Result;
            return _currentAccount;
        }

        public async Task<List<RoomsResult>> GetRooms()
        {
            var result =
                await SocketCall<MethodCallResponse<List<RoomsResult>>>(new MethodCallMessage<object>("rooms/get"));
            return result.Result;
        }

        public async Task<List<SubscriptionResult>> GetSubscription()
        {
            var result =
                await SocketCall<MethodCallResponse<List<SubscriptionResult>>>(
                    new MethodCallMessage<object>("subscriptions/get"));
            return result.Result;
        }

        public async Task<HistoryResult> LoadHistory(string roomId, int count, DateTime lastRefresh,
            DateTime? since = null)
        {
            var result = await SocketCall<MethodCallResponse<HistoryResult>>(new MethodCallMessage<object?>(
                "loadHistory",
                roomId,
                since?.ToDateModel(),
                count,
                lastRefresh.ToDateModel()
            ));
            return result.Result;
        }

        public async Task<List<MessageData>> GetMessages(params string[] id)
        {
            var result = await SocketCall<MethodCallResponse<List<MessageData>>>(new MethodCallMessage<string>(
                "getMessages",
                id
            ));
            return result.Result;
        }

        public async Task<List<IEmojiData>> GetEmojis()
        {
            var assembly = typeof(RocketClient).GetTypeInfo().Assembly;
            using var resource = assembly.GetManifestResourceStream("Rocket.Chat.Net.Emoji.emoji.json");
            using var streamReader = new StreamReader(resource);
            using var reader = new JsonTextReader(streamReader);
            var emojis = new JsonSerializer().Deserialize<Dictionary<string, EmojiData>>(reader);
            var result = emojis.Values.OfType<IEmojiData>().ToList();
            using var client = CreateHttpClient();
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
