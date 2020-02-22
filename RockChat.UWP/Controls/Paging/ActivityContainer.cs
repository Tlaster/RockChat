using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace RockChat.Controls.Paging
{
    partial class ActivityContainer
    {
        private void UWPInit()
        {
            Application.Current.Suspending += OnApplicationSuspending;
            Application.Current.Resuming += OnApplicationResuming;
        }

        private void OnApplicationResuming(object sender, object e)
        {
            CurrentActivity?.OnApplicationResume();
        }

        private void OnApplicationSuspending(object sender, SuspendingEventArgs e)
        {
            CurrentActivity?.OnApplicationSuspend();
        }
    }

    partial class Activity
    {
        protected internal virtual void OnApplicationResume()
        {

        }

        protected internal virtual void OnApplicationSuspend()
        {

        }
    }
}
