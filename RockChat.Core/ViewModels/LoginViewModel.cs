using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RockChat.Core.Common;
using RockChat.Core.Models;
using RockChat.Core.PlatformServices;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;

namespace RockChat.Core.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private RocketClient? _currentClient;

        public LoginViewModel()
        {
            Init();
        }

        public string? Host { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public IWebProxy? Proxy { get; set; }

        [DependsOn(nameof(UserName), nameof(Password))]
        public bool LoginEnabled => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

        public bool IsRocketChatServer { get; private set; }
        public bool AllowRegistration { get; private set; }
        public bool ShowLogin { get; private set; }
        public bool AllowPasswordReset { get; private set; }
        public List<InstanceModel> Instances => RockApp.Current.ActiveInstance;
        public List<AuthService> AuthServices { get; private set; } = new List<AuthService>();

        private void Init()
        {
            RockApp.Current.GetAllInstance();
            OnPropertyChanged(nameof(Instances));
        }

        public void Reset()
        {
            IsRocketChatServer = false;
            AllowPasswordReset = false;
            AllowRegistration = false;
            ShowLogin = false;
            UserName = string.Empty;
            Password = string.Empty;
            Proxy = null;
            AuthServices = new List<AuthService>();
        }

        public async Task CheckServer()
        {
            if (string.IsNullOrEmpty(Host))
            {
                return;
            }

            if (_currentClient != null)
            {
                _currentClient.Dispose();
                _currentClient = null;
            }

            var client = new RocketClient(Host, this.Platform<IDispatcher>()) {Proxy = Proxy};
            await client.Connect();
            //var result = await client.GetServerInformation();
            var settings = await client.GetPublicSettings();
            AuthServices = await client.SettingsOAuth();
            IsRocketChatServer = /*result != null &&*/ settings != null;
            if (settings != null)
            {
                AllowRegistration = settings["Accounts_RegistrationForm"].Value<string>() == "Public";
                ShowLogin = settings["Accounts_ShowFormLogin"].Value<bool>();
                AllowPasswordReset = settings["Accounts_PasswordReset"].Value<bool>();
                _currentClient = client;
            }
        }

        public void UpdateSettings(InstanceModel model)
        {
            var settings = this.Platform<ISettings>();
            var index = Instances.FindIndex(it => it.CreateAt == model.CreateAt);
            if (index > 0)
            {
                Instances[index] = model;
            }

            settings.Set("instance", Instances);
            RockApp.Current.GetAllInstance();
            OnPropertyChanged(nameof(Instances));
        }

        public async Task<InstanceModel> Login()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                throw new ArgumentNullException($"{nameof(UserName)} and {nameof(Password)} can not be null or empty");
            }

            var result = await _currentClient.Login(UserName, Password);
            var model = new InstanceModel
            {
                CreateAt = DateTime.UtcNow,
                Client = _currentClient,
                Expires = result.TokenExpires.ToDateTime(),
                Host = Host,
                ImType = IMType.RocketChat,
                Token = result.Token,
                UserId = result.Id
            };
            RockApp.Current.AddInstance(model);
            return model;
        }

        public async Task Login(InstanceModel instance)
        {
            if (instance.Client != null)
            {
                throw new RocketClientException("Already Login");
            }
            
            Host = instance.Host;
            Proxy = instance.ProxySettings?.ToWebProxy();
            await CheckServer();
            var result = await _currentClient.Login(instance.Token);
            instance.Client = _currentClient;
        }

        public async Task<InstanceModel> Login(string credentialToken, string credentialSecret)
        {
            if (string.IsNullOrEmpty(credentialToken) || string.IsNullOrEmpty(credentialSecret))
            {
                throw new ArgumentNullException($"{nameof(credentialToken)} and {nameof(credentialSecret)} can not be null or empty");
            }

            var result = await _currentClient.OAuthLogin(credentialToken, credentialSecret);
            var userInfo = await _currentClient.GetUserInfo(result.Id);
            if (string.IsNullOrEmpty(userInfo.UserName))
            {
                var usernameSuggestion = await _currentClient.GetUsernameSuggestion();
                var name = await this.Platform<IDialog>().ShowInput(new InputDialogData
                {
                    InputDefaultValue = usernameSuggestion,
                    Title = "Register UserName",
                    Content = "The username is used to allow others to mention you in messages."
                });
                if (string.IsNullOrEmpty(name))
                {
                    throw new TaskCanceledException();
                }

                await _currentClient.SetUsername(name);
            }
            
            var model = new InstanceModel
            {
                CreateAt = DateTime.UtcNow,
                Client = _currentClient,
                Expires = result.TokenExpires.ToDateTime(),
                Host = Host,
                ImType = IMType.RocketChat,
                Token = result.Token,
                UserId = result.Id
            };
            RockApp.Current.AddInstance(model);
            return model;
        }

        public (string requestUri, string callbackUri) OAuth(AuthService authService)
        {
            switch (authService.Name)
            {
                case "facebook":
                {
                    var endpoint = "https://m.facebook.com/v2.9/dialog/oauth";
                    var redirect_uri = $"https://{Host}/_oauth/facebook?close";
                    var scope = "email";
                    var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&scope={scope}&state={state}&display=touch";
                    return ($"{endpoint}{@params}", redirect_uri);
                }
                case "github":
                {
                    var endpoint = $"https://github.com/login?client_id={authService.ClientId}&return_to={WebUtility.UrlEncode("/login/oauth/authorize")}";
                    var redirect_uri = $"https://{Host}/_oauth/github?close";
                    var scope = "user:email";
                    var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&scope={scope}&state={state}";
                    return ($"{endpoint}{WebUtility.UrlEncode(@params)}", redirect_uri);
                }
                case "gitlab":
                {
                    var baseURL = "https://gitlab.com";
		            var endpoint = $"{baseURL}/oauth/authorize";
		            var redirect_uri = $"https://{Host}/_oauth/gitlab?close";
		            var scope = "read_user";
		            var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&scope={scope}&state={state}&response_type=code";
                    return ($"{endpoint}{@params}", redirect_uri);
                }
                case "google":
                {
                    var endpoint = "https://accounts.google.com/o/oauth2/auth";
                    var redirect_uri = $"https://{Host}/_oauth/google?close";
                    var scope = "email";
                    var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&scope={scope}&state={state}&response_type=code";
                    return ($"{endpoint}{@params}", redirect_uri);
                }
                case "linkedin":
                {
                    var endpoint = "https://www.linkedin.com/uas/oauth2/authorization";
                    var redirect_uri = $"https://{Host}/_oauth/linkedin?close";
                    var scope = "r_emailaddress";
                    var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&scope={scope}&state={state}&response_type=code";
                    return ($"{endpoint}{@params}", redirect_uri);
                }
                case "meteor-developer":
                {
                    var endpoint = "https://www.meteor.com/oauth2/authorize";
                    var redirect_uri = $"https://{Host}/_oauth/meteor-developer";
                    var state = GetOAuthState();
                    var @params =
                        $"?client_id={authService.ClientId}&redirect_uri={redirect_uri}&state={state}&response_type=code";
                    return ($"{endpoint}{@params}", redirect_uri);
                }
                case "twitter":
                {
                    var state = GetOAuthState();
                    return ($"https://{Host}/_oauth/twitter/?requestTokenAndRedirect=true&state={state}", string.Empty);
                }
                //case "wordpress":
                //{
                //    var endpoint = $"{ serverURL }/oauth/authorize";
                //    var redirect_uri = $"{ server }/_oauth/wordpress?close";
                //    var scope = "openid";
                //    var state = GetOAuthState();
                //    var @params = "?client_id={ clientId }&redirect_uri={ redirect_uri }&scope={ scope }&state={ state }&response_type=code";
                //}
                default:
                    throw new NotSupportedException();
            }
        }

        private string GetOAuthState()
        {
            var credentialToken = OAuthRandom(43);
            var json = new {loginStyle = "popup", credentialToken, isCordova = true}.ToJson();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        private string OAuthRandom(int length)
        {
            var text = string.Empty;
            var random = new Random(length);
            const string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            for (var i = 0; i < length; i++)
            {
                text += possible.ElementAtOrDefault((int) Math.Floor((decimal) (random.NextDouble() * possible.Length)));
            }
            return text;
        }
    }
}