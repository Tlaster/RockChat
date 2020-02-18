using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Input;
using RockChat.UWP.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    internal class ChatBoxStateManager
    {
        private readonly ChatBox _chatBox;
        private readonly string _id;
        private MessageData _currentEditingMessage;

        public ChatBoxStateManager(ChatBox chatBox, string id)
        {
            _chatBox = chatBox;
            _id = id;
            _chatBox.PreviewKeyDown += OnPreviewKeyDown;
        }

        public bool IsEditingMessage { get; set; }
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

        private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (Messages?.Any() == true)
            {
                //TODO: Multiple line detection
                if (e.Key == VirtualKey.Down)
                {
                    if (IsEditingMessage)
                    {
                        e.Handled = true;
                        var item = Messages.Skip(Messages.IndexOf(CurrentEditingMessage) + 1)
                            .FirstOrDefault(it => it.User.Id == _id && string.IsNullOrEmpty(it.Type));
                        CurrentEditingMessage = item ?? null;
                    }
                }

                if (e.Key == VirtualKey.Up)
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
            }
        }
    }
}