using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using Microsoft.Toolkit.Parsers.Markdown.Render;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Render;

namespace RockChat.UWP.Controls
{
    class MarkdownTextBlockEx : MarkdownTextBlock
    {
        public MarkdownTextBlockEx()
        {
            SetRenderer<Renderer>();
        }
    }

    class Renderer : MarkdownRenderer
    {
        public Renderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver, ICodeBlockResolver codeBlockResolver) : base(document, linkRegister, imageResolver, codeBlockResolver)
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

    class ImageEx2 : ImageEx
    {
        
    }
}
