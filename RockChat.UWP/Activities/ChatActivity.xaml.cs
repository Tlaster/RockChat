using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
                ViewModel = new ChatViewModel(id) {RoomMessage = OnRoomMessage};
                WithHostConverter.Host = ViewModel.Host;
                AdvancedCollectionView = new AdvancedCollectionView(ViewModel.Rooms, true);
                AdvancedCollectionView.SortDescriptions.Add(new SortDescription("UpdateAt", SortDirection.Descending));
            }
        }

        private bool OnRoomMessage(string arg)
        {
            return false;
            //return ChatMasterDetailView.CurrentItem is RoomModel model && model.RoomsResult.Id == arg;
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
            }
        }

        private async void ChatBox_UploadFile(object sender, Windows.Storage.StorageFile e)
        {
            if (sender is ChatBox chatBox)
            {
                switch (chatBox.DataContext)
                {
                    case RoomModel model:
                        await UploadFile(e, model);
                        break;
                    case ThreadMessageViewModel viewModel:
                        await UploadFile(e, viewModel);
                        break;
                }
            }
        }

        private async Task UploadFile(StorageFile file, ThreadMessageViewModel viewModel)
        {
            await UploadFile(file, viewModel.Room, viewModel.Tmid);
        }

        private async Task UploadFile(StorageFile file, RoomModel model, string? tmid = null)
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
                ViewModel.SendFile(model, new FileInfo(file.Path), data.Name, data.Description, tmid);
            }
        }

        private async void RoomOnDrop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.FirstOrDefault() is StorageFile file)
                    {
                        var cacheFile = await file.CopyAsync(ApplicationData.Current.LocalCacheFolder);
                        switch (element.Tag)
                        {
                            case RoomModel model:
                                UploadFile(cacheFile, model);
                                break;
                            case ThreadMessageViewModel viewModel:
                                UploadFile(cacheFile, viewModel);
                                break;
                        }
                    }
                }   
            }
        }

        private void RoomOnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void ChatListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MessageData data)
            {
                if (!string.IsNullOrEmpty(data.Tmid) && ChatMasterDetailView.CurrentItem is RoomModel model)
                {
                    ExtraPane.IsPaneOpen = true;
                    PaneContent.ContentTemplate = RoomThreadChatTemplate;
                    PaneContent.Content = new ThreadMessageViewModel(data.Tmid, model, ViewModel.Instance);
                }
            }
        }

        private void ThreadMessage_Commit(object sender, string e)
        {
            if (sender is ChatBox chatBox && chatBox.DataContext is ThreadMessageViewModel viewModel)
            {
                ViewModel.SendText(viewModel.Room, e, viewModel.Tmid);
            }
        }

        private void ExtraPane_PaneClosed(SplitView sender, object args)
        {
            if (PaneContent.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
