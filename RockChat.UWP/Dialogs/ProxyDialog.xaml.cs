using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Dialogs
{
    public sealed partial class ProxyDialog : ContentDialog
    {
        public class ProxyData
        {
            public string Url { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }

            public WebProxy ToWebProxy()
            {
                return  new WebProxy(new Uri(Url))
                {
                    Credentials = new NetworkCredential(UserName, Password)
                };
            }

            public static ProxyData FromWebProxy(IWebProxy proxy)
            {
                var result = new ProxyData();
                if (proxy is WebProxy webProxy)
                {
                    result.Url = webProxy.Address.ToString();
                    if (webProxy.Credentials is NetworkCredential networkCredential)
                    {
                        result.Password = networkCredential.Password;
                        result.UserName = networkCredential.UserName;
                    }
                }

                return result;
            }
        }

        public ProxyData Data { get; }

        public ProxyDialog(ProxyData? data = null)
        {
            Data = data ?? new ProxyData();
            this.InitializeComponent();
        }
    }
}
