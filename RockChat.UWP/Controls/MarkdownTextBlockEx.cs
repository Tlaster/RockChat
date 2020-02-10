using System;
using System.Linq;
using System.ServiceModel;
using Windows.Storage;
using Windows.UI.Xaml;
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

        private void OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            
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

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MarkdownTextBlockEx).RenderMessage();
        }

        private void RenderMessage()
        {
            var markdown = Content;
            markdown = markdown.Replace("\n", $"{Environment.NewLine}{Environment.NewLine}");
            if (Message is MessageData data)
            {
                if (data.Mentions?.Any() == true)
                {
                    data.Mentions.ForEach(user => markdown = markdown.Replace($"@{user.UserName}", $"[{user.Name}](/user/{user.Id})"));
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

        protected override void RenderMarkdownLink(MarkdownLinkInline element, IRenderContext context)
        {
            //Workaround for [ ](https://rocket.chat)
            if (element.Inlines.Count != 1 || element.Inlines.First().Type != MarkdownInlineType.TextRun ||
                !(element.Inlines.First() is TextRunInline textRunInline) ||
                !string.IsNullOrWhiteSpace(textRunInline.Text))
            {
                base.RenderMarkdownLink(element, context);
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