using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using Microsoft.Toolkit.Parsers.Markdown.Render;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Render;
using Microsoft.UI.Xaml.Controls;

namespace RockChat.UWP.Controls
{
    internal class MarkdownTextBlockEx : MarkdownTextBlock
    {
        public MarkdownTextBlockEx()
        {
            SetRenderer<Renderer>();
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
    }
}