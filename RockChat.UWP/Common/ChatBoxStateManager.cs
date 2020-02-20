using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using RockChat.UWP.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    internal class ChatBoxStateManager
    {
        private readonly ChatBox _chatBox;
        private readonly string _id;
        private MessageData _currentEditingMessage;
        private bool _isEditingMessage;

        public ChatBoxStateManager(ChatBox chatBox, string id)
        {
            _chatBox = chatBox;
            _id = id;
            _chatBox.PreviewKeyDown += OnPreviewKeyDown;
        }

        public bool IsEditingMessage
        {
            get => _isEditingMessage;
            private set
            {
                _isEditingMessage = value;
                if (_chatBox.Parent is Control control)
                {
                    //TODO: Not works
                    var result = VisualStateManager.GoToState(control, value ? "Editing" : "Normal", true);
                }
            }
        }

        public MessageData? ThreadMessage { get; set; }
        public IList<MessageData> Messages { get; set; }

        public MessageData CurrentEditingMessage
        {
            get => _currentEditingMessage;
            set
            {
                if (_currentEditingMessage != null)
                {
                    _currentEditingMessage.IsSelected = false;
                }

                _currentEditingMessage = value;
                if (_currentEditingMessage != null)
                {
                    _currentEditingMessage.IsSelected = true;
                }

                IsEditingMessage = value != null;
                _chatBox.Text = value?.Text ?? string.Empty;
            }
        }

        public void RequestFocus()
        {
            _chatBox.Focus(FocusState.Programmatic);
        }

        private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (Messages?.Any() == true)
            {
                //TODO: Multiple line detection
                if (e.Key == VirtualKey.Down && _chatBox.SelectionStart + _chatBox.SelectionLength == _chatBox.Text.Length)
                {
                    if (IsEditingMessage)
                    {
                        e.Handled = true;
                        var item = Messages.Skip(Messages.IndexOf(CurrentEditingMessage) + 1)
                            .FirstOrDefault(it => it.User.Id == _id && string.IsNullOrEmpty(it.Type));
                        CurrentEditingMessage = item ?? null;
                    }
                }

                if (e.Key == VirtualKey.Up && _chatBox.SelectionStart == 0)
                {
                    if (IsEditingMessage)
                    {
                        e.Handled = true;
                        var item = Messages.SkipLast(Messages.Count - Messages.IndexOf(CurrentEditingMessage))
                            .LastOrDefault(it => it.User.Id == _id && string.IsNullOrEmpty(it.Type));
                        CurrentEditingMessage = item ?? null;
                    }
                    else
                    {
                        var item = Messages.LastOrDefault(it => it.User.Id == _id && string.IsNullOrEmpty(it.Type));
                        if (item != null)
                        {
                            e.Handled = true;
                            CurrentEditingMessage = item;
                        }
                    }
                }

                if (e.Key == VirtualKey.Escape)
                {
                    if (IsEditingMessage)
                    {
                        e.Handled = true;
                        CurrentEditingMessage = null;
                    }
                }
            }
        }
    }
}