using System;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RockChat.UWP.Common;

namespace RockChat.UWP.Controls
{
    class ChatBox : TextBox
    {
        public event EventHandler<string>? Commit;
        public event EventHandler<StorageFile>? UploadFile; 
        public ChatBox()
        {
            DefaultStyleKey = typeof(TextBox);
            AcceptsReturn = true;
            Paste += OnPaste;
        }

        private async void OnPaste(object sender, TextControlPasteEventArgs e)
        {
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                e.Handled = true;
                var bitmap = await dataPackageView.GetBitmapAsync();
                var file = await bitmap.SaveCacheFile($"Clipboard - {DateTime.Now:yyyy-mm-dd HH-mm-ss}");
                UploadFile?.Invoke(this, file);
            }
            else if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                e.Handled = true;
                var files = (await dataPackageView.GetStorageItemsAsync())
                    .Select(it => it as StorageFile)
                    .ToArray();
                UploadFile?.Invoke(this, files.FirstOrDefault());
            }
        }

        protected override void OnPreviewKeyDown(KeyRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)&& e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                Text += Environment.NewLine;
                SelectionStart = Text.Length;
            }
            else if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                Commit?.Invoke(this, Text);
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }
    }
}