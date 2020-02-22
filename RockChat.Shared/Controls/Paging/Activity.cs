using System;
#if ANDROID
using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.Util;
#endif
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace RockChat.Controls.Paging
{
    public partial class Activity
#if WINDOWS_UWP
        : UserControl
#elif ANDROID
        : FrameLayout
#endif
    {

#if ANDROID
        protected Activity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public Activity(Context context) : base(context)
        {
        }

        public Activity(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public Activity(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public Activity(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected void SetContentView(View view)
        {
            AddView(view);
        }
#endif

#if WINDOWS_UWP
        public Activity()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }
#endif

        public NavigationCacheMode NavigationCacheMode { get; set; } = NavigationCacheMode.Required;
        public IActivityTransition ActivityTransition { get; set; }
        public ActivityContainer Container { get; internal set; }

        protected void Finish()
        {
            Container.FinishActivity(this);
        }

        protected void StartActivity(Type type, object parameter = null)
        {
            Container.Navigate(type, parameter);
        }

        protected void StartActivity<T>(object parameter = null) where T : Activity
        {
            StartActivity(typeof(T), parameter);
        }

        protected internal virtual void OnBackPressed()
        {
            Finish();
        }

        protected internal virtual void OnCreate(object parameter)
        {
        }

        protected internal virtual void OnStart()
        {
        }

        protected internal virtual void OnRestart()
        {
        }

        protected internal virtual void OnStop()
        {
        }

        protected internal virtual void OnResume()
        {
        }

        protected internal virtual void OnPause()
        {
        }

        protected internal virtual void OnClose()
        {
        }

        protected internal virtual void OnDestroy()
        {
        }
    }
}