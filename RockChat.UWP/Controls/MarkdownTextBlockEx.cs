using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using Microsoft.Toolkit.Parsers.Markdown.Render;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Render;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Controls
{
    internal class MarkdownTextBlockEx : MarkdownTextBlock
    {
        private static readonly Regex _emojiRegex = new Regex(":([^:]*):", RegexOptions.Compiled);
        public static readonly DependencyProperty EmojiDatasProperty = DependencyProperty.Register(
            nameof(EmojiDatas), typeof(IList<IEmojiData>), typeof(MarkdownTextBlockEx),
            new PropertyMetadata(default(IList<IEmojiData>)));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(IMessage), typeof(MarkdownTextBlockEx),
            new PropertyMetadata(default(IMessage), PropertyChangedCallback));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(string), typeof(MarkdownTextBlockEx),
            new PropertyMetadata(default(string), PropertyChangedCallback));

        public MarkdownTextBlockEx()
        {
            SetRenderer<Renderer>();
            LinkClicked += OnLinkClicked;
        }

        public IList<IEmojiData> EmojiDatas
        {
            get => (IList<IEmojiData>) GetValue(EmojiDatasProperty);
            set => SetValue(EmojiDatasProperty, value);
        }

        public IMessage Message
        {
            get => (IMessage) GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Content
        {
            get => (string) GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private void OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.Link.StartsWith("http"))
            {
                Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MarkdownTextBlockEx).RenderMessage();
        }

        private void RenderMessage()
        {
            if (string.IsNullOrEmpty(Content))
            {
                Text = Content;
                return;
            }

            var markdown = Content;
            if (EmojiDatas?.Any() == true)
            {
                var matches = _emojiRegex.Matches(markdown).GroupBy(it => it.Value).Select(it => it.First());
                foreach (var match in matches)
                {
                    var emoji = EmojiDatas.FirstOrDefault(it => it.Validate(match.Value));
                    if (emoji != null)
                    {
                        var path = emoji switch
                        {
                            RemoteEmojiData _ => emoji.Path,
                            EmojiData _ => $"ms-appx:///Assets/Emoji/{emoji.Path}.png",
                            _ => throw new NotSupportedException()
                        };
                        markdown = markdown.Replace(match.Value, $"![:{emoji.Name}:]({path})");
                    }
                }
            }

            markdown = markdown.Replace("\n", $"{Environment.NewLine}{Environment.NewLine}");
            if (Message is MessageData data)
            {
                if (data.Mentions?.Any() == true)
                {
                    data.Mentions.ForEach(user =>
                        markdown = markdown.Replace($"@{user.UserName}", $"[@{user.UserName}](/user/{user.Id})"));
                }

                if (data.Urls?.Any() == true)
                {
                    data.Urls.ForEach(url => markdown = markdown.Replace(url.Url, $"[{url.Url}]({url.Url})"));
                }
            }

            Text = markdown;
        }
    }

    internal class Renderer : MarkdownRenderer
    {
        public Renderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver,
            ICodeBlockResolver codeBlockResolver) : base(document, linkRegister, imageResolver, codeBlockResolver)
        {
        }

        protected override void RenderImage(ImageInline element, IRenderContext context)
        {
            if (element.Tooltip.StartsWith(":") && element.Tooltip.EndsWith(":"))
            {
                var localContext = context as InlineRenderContext;
                var name = element.Tooltip;
                var img = new ImageEx
                {
                    Source = element.RenderUrl,
                    Width = FontSize,
                    Height = FontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Stretch = Stretch.UniformToFill
                };
                var container = new InlineUIContainer {Child = img};
                ToolTipService.SetToolTip(img, name);
                localContext?.InlineCollection.Add(container);
            }
            else
            {
                base.RenderImage(element, context);
            }
            
        }

        protected override void RenderMarkdownLink(MarkdownLinkInline element, IRenderContext context)
        {
            //Workaround for [ ](https://rocket.chat)
            if (element.Inlines.Count != 1 || element.Inlines.First().Type != MarkdownInlineType.TextRun ||
                !(element.Inlines.First() is TextRunInline textRunInline) ||
                !string.IsNullOrWhiteSpace(textRunInline.Text))
            {
                if (element.Url.StartsWith("/user/"))
                {
                    base.RenderMarkdownLink(element, context);
                    //if (context is InlineRenderContext localContext)
                    //{
                    //    var link = new HyperlinkButton
                    //    {
                    //        Content = element.Inlines.OfType<TextRunInline>()?.FirstOrDefault()?.Text
                    //    };
                    //    var container = new InlineUIContainer {Child = link};
                    //    ToolTipService.SetToolTip(link, element.Tooltip ?? element.Url);
                    //    localContext.InlineCollection.Add(container);
                    //}
                }
                else
                {
                    base.RenderMarkdownLink(element, context);
                }
            }
        }


        protected override void RenderCodeRun(CodeInline element, IRenderContext context)
        {
            //Workaround for [`code`](https://rocket.chat)
            if (context is InlineRenderContext renderContext && renderContext.WithinHyperlink)
            {
                base.RenderTextRun(new TextRunInline
                {
                    Text = element.Text
                }, context);
            }
            else
            {
                base.RenderCodeRun(element, context);
            }
        }
    }
}