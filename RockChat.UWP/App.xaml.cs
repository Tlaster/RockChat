using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Toolkit.Uwp.UI;
using RockChat.Core;
using RockChat.Core.PlatformServices;
using RockChat.UWP.PlatformServices;
using Rocket.Chat.Net.Common;

namespace RockChat.UWP
{
    sealed partial class App 
    {
        public App()
        {
            this.InitializeComponent();
            RockApp.Init(builder =>
            {
                builder.RegisterInstance(new Settings()).As<ISettings>();
                builder.RegisterInstance(new Dispatcher()).As<IDispatcher>();
                builder.RegisterInstance(new Notification()).As<INotification>();
            });
            ImageCache.Instance.InitializeAsync();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is RootView))
            {
                Window.Current.Content = new RootView();
            }

            if (!e.PrelaunchActivated)
            {
                Window.Current.Activate();
            }
        }
    }
}
