﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Rocket.Chat.Net.Models;

namespace RockChat.UWP.Common
{
    class AttachmentDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate MessageTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (!(item is Attachment attachment))
            {
                return DefaultTemplate;
            }

            if (attachment.ImageType != null)
            {
                return ImageTemplate;
            }

            if (attachment.VideoType != null)
            {
                return VideoTemplate;
            }

            if (attachment.AuthorName != null)
            {
                return MessageTemplate;
            }

            if (attachment.Type == "file" && attachment.TitleLinkDownload == true)
            {
                return FileTemplate;
            }

            return DefaultTemplate;
        }
    }
}
