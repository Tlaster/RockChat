using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
using RockChat.UWP.Controls;
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
        public AdvancedCollectionView AdvancedCollectionView { get; private set; }

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

        private void ChatBox_Commit(object sender, string text)
        {
            if (sender is ChatBox chatBox && chatBox.DataContext is RoomModel model)
            {
                ViewModel.SendText(model, text);
                chatBox.Text = string.Empty;
            }
        }

        private async void ChatBox_UploadFile(object sender, Windows.Storage.StorageFile e)
        {
            if (sender is ChatBox chatBox && chatBox.DataContext is RoomModel model)
            {
                await UploadFile(e, model);
            }
        }

        private async Task UploadFile(StorageFile file, RoomModel model)
        {
            var data = new FileMessageDialog.FileUploadData
            {
                Name = file.Name,
                File = file
            };
            var dialog = new FileMessageDialog(data);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.SendFile(model, new FileInfo(file.Path), data.Name, data.Description);
            }
        }

        private async void RoomOnDrop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is RoomModel model)
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.FirstOrDefault() is StorageFile file)
                    {
                        var cacheFile = await file.CopyAsync(ApplicationData.Current.LocalCacheFolder);
                        UploadFile(cacheFile, model);
                    }
                }   
            }
        }

        private void RoomOnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }
}
