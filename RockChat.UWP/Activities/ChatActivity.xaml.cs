using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WebUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.UI.Xaml.Controls;
using RockChat.Core.Models;
using RockChat.Core.ViewModels;
using RockChat.UWP.Common;
using RockChat.UWP.Controls;
using RockChat.UWP.Dialogs;
using Rocket.Chat.Net;
using Rocket.Chat.Net.Models;
using NavigationViewOverflowLabelMode = Microsoft.UI.Xaml.Controls.NavigationViewOverflowLabelMode;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Activities
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatActivity
    {
        private ChatBoxStateManager _chatBoxStateManager;
        private ChatBoxStateManager _threadChatBoxStateManager;
        public ChatViewModel ViewModel { get; private set; }
        public AdvancedCollectionView AdvancedCollectionView { get; private set; }

        public ChatActivity()
        {
            this.InitializeComponent();
        }

        protected internal override void OnCreate(object parameter)
        {
            base.OnCreate(parameter);
            if (parameter is InstanceModel instance)
            {
                ViewModel = new ChatViewModel(instance) {RoomMessage = OnRoomMessage};
                WithHostConverter.Host = ViewModel.Host;
                AdvancedCollectionView = new AdvancedCollectionView(ViewModel.Rooms, true);
                AdvancedCollectionView.SortDescriptions.Add(new SortDescription("UpdateAt", SortDirection.Descending));
            }
        }

        protected internal override void OnApplicationSuspend()
        {
            base.OnApplicationSuspend();
            ViewModel.Suspend();
        }

        protected internal override void OnApplicationResume()
        {
            base.OnApplicationResume();
            ViewModel.Resume();
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
                if (_chatBoxStateManager.IsEditingMessage)
                {
                    ViewModel.UpdateMessage(_chatBoxStateManager.CurrentEditingMessage, text);
                    _chatBoxStateManager.CurrentEditingMessage = null;
                }
                else
                {
                    ViewModel.SendText(model, text, _chatBoxStateManager?.ThreadMessage?.Id);
                    CommentTeachingTip.IsOpen = false;
                }
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
                await ViewModel.SendFile(model, new FileInfo(file.Path), data.Name, data.Description, tmid);
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
                if (_threadChatBoxStateManager.IsEditingMessage)
                {
                    ViewModel.UpdateMessage(_threadChatBoxStateManager.CurrentEditingMessage, e);
                    _threadChatBoxStateManager.CurrentEditingMessage = null;
                }
                else
                {
                    ViewModel.SendText(viewModel.Room, e, viewModel.Tmid);
                }
            }
        }

        private void ExtraPane_PaneClosed(SplitView sender, object args)
        {
            if (PaneContent.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void ImageEx_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Attachment attachment)
            {
                e.Handled = true;
                StartActivity<ImageActivity>(new ImageActivity.Data
                {
                    Attachment = attachment,
                    Host = ViewModel.Host
                });
            }
        }

        private void ChatBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ChatBox chatBox)
            {
                chatBox.Loaded -= ChatBox_Loaded;
                _chatBoxStateManager = new ChatBoxStateManager(chatBox, ViewModel.Instance.UserId);
                CommentTeachingTip.Target = chatBox;
            }
        }

        private void ChatBox_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender is ChatBox chatBox && chatBox.DataContext is RoomModel model)
            {
                _chatBoxStateManager.CurrentEditingMessage = null;
                _chatBoxStateManager.Messages = model.Messages;
            }
        }

        private void ThreadChatBox_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender is ChatBox chatBox && chatBox.DataContext is ThreadMessageViewModel model)
            {
                if (_threadChatBoxStateManager == null)
                {
                    _threadChatBoxStateManager = new ChatBoxStateManager(chatBox, ViewModel.Instance.UserId);
                }

                _threadChatBoxStateManager.CurrentEditingMessage = null;
                _threadChatBoxStateManager.Messages = model.Message;
            }
        }

        private void ThreadChatBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ChatBox chatBox)
            {
                chatBox.Loaded -= ThreadChatBox_Loaded;
                //_threadChatBoxStateManager = new ChatBoxStateManager(chatBox, ViewModel.Instance.UserId);
            }
        }

        private void Comment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is MessageData data)
            {
                _chatBoxStateManager.ThreadMessage = data;
                CommentTeachingTip.Title = data.Name;
                CommentTeachingTip.Subtitle = data.Text;
                CommentTeachingTip.IsOpen = true;
                _chatBoxStateManager.RequestFocus();
            }
        }

        private void CommentTeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            if (_chatBoxStateManager != null)
            {
                _chatBoxStateManager.ThreadMessage = null;
            }
        }

        private void ReactionItemsControlTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement control && control.DataContext is MessageData messageData && 
                e.OriginalSource is FrameworkElement element && element.DataContext is KeyValuePair<string, Reaction> item)
            {
                e.Handled = true;
                ViewModel.SetReaction(messageData, item.Key);
            }
        }

        private void ReactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && EmojiFlyout.Content is FrameworkElement emojiFlyoutContent)
            {
                emojiFlyoutContent.Tag = element.Tag;
                EmojiFlyout.ShowAt(element);
            }
        }

        private void ReactionEmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (sender is FrameworkElement element && element.FindAscendant<Pivot>()?.Tag is MessageData message && e.ClickedItem is IEmojiData emojiData)
            {
                ViewModel.SetReaction(message, emojiData.Symbol);
                EmojiFlyout.Hide();
            } 
        }

        private void EmojiFlyout_Closed(object sender, object e)
        {
            if (EmojiFlyout.Content is FrameworkElement emojiFlyoutContent)
            {
                emojiFlyoutContent.Tag = null;
            }
        }
    }
}
