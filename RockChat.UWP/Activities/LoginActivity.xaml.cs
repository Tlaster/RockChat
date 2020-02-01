using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RockChat.Core.ViewModels;
using Rocket.Chat.Net;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Activities
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginActivity
    {
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
                try
                {
                    await ViewModel.Login(id);
                    StartActivity<ChatActivity>(id);
                    Finish();
                }
                catch (RocketClientException e)
                {
                    
                }
            }
        }

        private async void Login()
        {
            if (string.IsNullOrEmpty(ViewModel.UserName) || string.IsNullOrEmpty(ViewModel.Password))
            {
                return;
            }

            try
            {
                var id= await ViewModel.Login();
                StartActivity<ChatActivity>(id);
                Finish();
            }
            catch (RocketClientException e)
            {
                
            }
        }
    }
}
