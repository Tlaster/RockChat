using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RockChat.Core.Models;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Common;
using Rocket.Chat.Net.Models;

namespace RockChat.Core.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private RocketClient _currentClient;
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        [DependsOn(nameof(UserName), nameof(Password))]
        public bool LoginEnabled => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
        public bool IsRocketChatServer { get; private set; }
        public bool AllowRegistration { get; private set; }
        public bool ShowLogin { get; private set; }
        public bool AllowPasswordReset { get; private set; }
        public IDictionary<Guid, InstanceModel> Instances => RockApp.Current.ActiveInstance;

        public LoginViewModel()
        {
            Init();
        }

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
            var client = new RocketClient(Host, this.Platform<IDispatcher>());
            await client.Connect();
            //var result = await client.GetServerInformation();
            var settings = await client.GetPublicSettings();
            IsRocketChatServer = /*result != null &&*/ settings != null;
            if (settings != null)
            {
                AllowRegistration = settings["Accounts_RegistrationForm"].Value<string>() == "Public";
                ShowLogin = settings["Accounts_ShowFormLogin"].Value<bool>();
                AllowPasswordReset = settings["Accounts_PasswordReset"].Value<bool>();
                _currentClient = client;
            }
            
        }

        public async Task<Guid> Login()
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
            return RockApp.Current.AddInstance(model);
        }

        public async Task Login(Guid id)
        {
            var instance = RockApp.Current.ActiveInstance[id];
            if (instance.Client != null)
            {
                throw new RocketClientException("Already Login");
            }
            Host = instance.Host;
            await CheckServer();
            var result = await _currentClient.Login(instance.Token);
            instance.Client = _currentClient;
        }
    }
}