using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Controls
{
    internal class EmojiView : ImageEx
    {
        public static readonly DependencyProperty EmojiDatasProperty = DependencyProperty.Register(
            nameof(EmojiDatas), typeof(List<IEmojiData>), typeof(EmojiView), new PropertyMetadata(default(List<IEmojiData>)));

        public static readonly DependencyProperty EmojiProperty = DependencyProperty.Register(
            nameof(Emoji), typeof(string), typeof(EmojiView), new PropertyMetadata(default, PropertyChangedCallback));

        public List<IEmojiData> EmojiDatas
        {
            get => (List<IEmojiData>) GetValue(EmojiDatasProperty);
            set => SetValue(EmojiDatasProperty, value);
        }

        public string Emoji
        {
            get => (string) GetValue(EmojiProperty);
            set => SetValue(EmojiProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmojiView view)
            {
                if (e.Property == EmojiProperty)
                {
                    view.OnEmojiChanged(e.NewValue as string);
                }
            }
        }

        private void OnEmojiChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                Source = null;
            }

            if (EmojiDatas == null || !EmojiDatas.Any())
            {
                return;
            }

            var emoji = EmojiDatas.FirstOrDefault(it => it.Validate(newValue));
            if (emoji == null)
            {
                return;
            }

            switch (emoji)
            {
                case RemoteEmojiData _:
                    Source = emoji.Path;
                    break;
                case EmojiData _:
                    Source = $"ms-appx:///Assets/Emoji/{emoji.Path}.png";
                    break;
            }
        }
    }
}