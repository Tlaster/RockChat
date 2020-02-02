using System;
using System.Collections;
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
using Microsoft.Toolkit.Uwp.UI;
using RockChat.Core.ViewModels;
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
                AdvancedCollectionView = new AdvancedCollectionView(ViewModel.Rooms, true);
                AdvancedCollectionView.SortDescriptions.Add(new SortDescription(SortDirection.Descending, new RoomModelComparer()));
            }
        }

        private void MasterDetailsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class RoomModelComparer : IComparer, IComparer<RoomModel>
    {
        public int Compare(RoomModel x, RoomModel y)
        {
            return CompareCore(x, y);
        }

        public int Compare(object x, object y)
        {
            if (!(x is RoomModel xmodel) || !(y is RoomModel ymodel))
            {
                return 0;
            }
            return CompareCore(xmodel, ymodel);
        }

        private int CompareCore(RoomModel x, RoomModel y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }


            return DateTime.Compare(x.RoomsResult.UpdatedAt.ToDateTime(), y.RoomsResult.UpdatedAt.ToDateTime());
        }

    }
}
