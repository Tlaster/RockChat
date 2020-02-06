using System;
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
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.UI.Xaml.Controls;
using RockChat.Core.ViewModels;
using RockChat.UWP.Common;
using Rocket.Chat.Net.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Activities
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatActivity
    {
        public ChatViewModel ViewModel { get; private set; }
        public AdvancedCollectionView AdvancedCollectionView { get; set; }

        public ChatActivity()
        {
            this.InitializeComponent();
        }

        protected internal override void OnCreate(object parameter)
        {
            base.OnCreate(parameter);
            if (parameter is Guid id)
            {
                ViewModel = new ChatViewModel(id);
                WithHostConverter.Host = ViewModel.Host;
                AdvancedCollectionView = new AdvancedCollectionView(ViewModel.Rooms, true);
                AdvancedCollectionView.SortDescriptions.Add(new SortDescription("UpdateAt", SortDirection.Descending));
            }
        }

        private void MasterDetailsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is RoomModel item)
            {
                ViewModel.FetchRoomHistory(item);
            }
        }
    }
}
