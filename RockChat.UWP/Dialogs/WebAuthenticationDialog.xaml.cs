using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Dialogs
{
    public sealed partial class WebAuthenticationDialog : ContentDialog
    {
        private readonly string _host;
        public WebAuthenticationDialog(string requestUri, string host)
        {
            this.InitializeComponent();
            webView.Navigate(new Uri(requestUri));
            this._host = host;
        }

        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var url = args.Uri.ToString();
            if (Regex.IsMatch(url, $"(?=.*(https://{_host}/))(?=.*(credentialToken))(?=.*(credentialSecret))"))
            {
                sender.Stop();
                var parts = url.Split('#');
                this.Result = JsonConvert.DeserializeObject<JObject>(parts[1]);
                Hide();
            }
        }

        public JObject? Result { get; private set; }
    }
}
