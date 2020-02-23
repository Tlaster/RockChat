using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Rocket.Chat.Net.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Activities
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageActivity
    {
        internal class Data
        {
            public Attachment Attachment { get; set; }
            public string Host { get; set; }
        }
        public Attachment Attachment { get; private set; }
        public ImageActivity()
        {
            this.InitializeComponent();
        }

        protected internal override void OnCreate(object parameter)
        {
            base.OnCreate(parameter);
            if (parameter is Data data)
            {
                this.WithHostConverter.Host = data.Host;
                this.Attachment = data.Attachment;
            }
        }

        protected override void OnPrepareConnectedAnimation(ConnectedAnimationService service)
        {
            service.PrepareToAnimate("image", Image).Configuration = new DirectConnectedAnimationConfiguration();
        }

        protected override void OnUsingConnectedAnimation(ConnectedAnimationService service)
        {
            service.GetAnimation("image")?.TryStart(Image);
        }
    }
}
