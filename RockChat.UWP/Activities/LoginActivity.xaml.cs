using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Diagnostics;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RockChat.Core.Models;
using RockChat.Core.ViewModels;
using RockChat.UWP.Controls;
using RockChat.UWP.Dialogs;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Models;
using VisualStateManager = Windows.UI.Xaml.VisualStateManager;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Activities
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginActivity
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                VisualStateManager.GoToState(this, value ? "Loading" : "Normal", true);
            }
        }

        public LoginViewModel ViewModel { get; } = new LoginViewModel();

        public LoginActivity()
        {
            this.InitializeComponent();
        }

        protected internal override async void OnCreate(object parameter)
        {
            base.OnCreate(parameter);

        }

        private async void ConnectServer()
        {
            IsLoading = true;
            await ViewModel.CheckServer();
            IsLoading = false;
        }

        private async void Login()
        {
            if (string.IsNullOrEmpty(ViewModel.UserName) || string.IsNullOrEmpty(ViewModel.Password))
            {
                return;
            }

            IsLoading = true;

            try
            {
                var id= await ViewModel.Login();
                StartActivity<ChatActivity>(id);
                Finish();
            }
            catch (RocketClientException e)
            {
                
            }

            IsLoading = false;
        }

        private async void LoginWithCurrent()
        {
            if (InstanceSelector.SelectedItem is KeyValuePair<Guid, InstanceModel> item)
            {
                IsLoading = true;
                try
                {
                    await ViewModel.Login(item.Key);
                    StartActivity<ChatActivity>(item.Key);
                    Finish();
                }
                catch (RocketClientException e)
                {
                }

                IsLoading = false;
            }
        }

        private async void AuthServiceButton_Click(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement element && element.Tag is AuthService authService)
            {
                IsLoading = true;

                try
                {
                    var authUrl = ViewModel.OAuth(authService);
                    var dialog = new WebAuthenticationDialog(authUrl.requestUri, ViewModel.Host);
                    await dialog.ShowAsync();
                    if (dialog.Result != null)
                    {
                        var credentialToken = dialog.Result.Value<string>("credentialToken");
                        var credentialSecret = dialog.Result.Value<string>("credentialSecret");
                        var id = await ViewModel.Login(credentialToken, credentialSecret);
                        StartActivity<ChatActivity>(id);
                        Finish();
                    }
                }
                catch (NotSupportedException)
                {

                }
                catch (TaskCanceledException e)
                {

                }
                catch (RocketClientException e)
                {

                }

                IsLoading = false;
            }
        }
    }
}
