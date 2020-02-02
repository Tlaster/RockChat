using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RockChat.Core.ViewModels;
using Rocket.Chat.Net;
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
            if (parameter is Guid id)
            {
                IsLoading = true;
                try
                {
                    await ViewModel.Login(id);
                    StartActivity<ChatActivity>(id);
                    Finish();
                }
                catch (RocketClientException e)
                {
                    
                }
                IsLoading = false;
            }
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

    }
}
