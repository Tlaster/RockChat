using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RockChat.Core;
using RockChat.UWP.Activities;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RockChat.UWP
{
    public sealed partial class RootView
    {
        public RootView()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            RootActivityContainer.BackStackChanged += RootActivityContainerOnBackStackChanged;
            RootActivityContainer.Navigate<LoginActivity>(RockApp.Current.GetDefaultInstance());
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
        }

        private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {

        }

        private void RootActivityContainerOnBackStackChanged(object sender, EventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                RootActivityContainer.CanGoBack 
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (RootActivityContainer.CanGoBack)
            {
                RootActivityContainer.OnBackPressed();
            }
        }
    }
}
