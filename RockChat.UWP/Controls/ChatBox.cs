using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace RockChat.UWP.Controls
{
    class ChatBox : TextBox
    {
        public event EventHandler<string>? Commit;  
        public ChatBox()
        {
            DefaultStyleKey = typeof(TextBox);
            AcceptsReturn = true;
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