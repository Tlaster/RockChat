using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#if ANDROID
using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
#endif
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace RockChat.Controls.Paging
{
    public partial class ActivityContainer
#if WINDOWS_UWP
        : Grid
#elif ANDROID
        : FrameLayout
#endif

            , INotifyPropertyChanged
    {
        private readonly ActivityStackManager _activityStackManager = new ActivityStackManager();
#if ANDROID
        protected ActivityContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public ActivityContainer(Context context) : base(context)
        {
            Init();
        }

        public ActivityContainer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public ActivityContainer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public ActivityContainer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }
#endif
#if WINDOWS_UWP
        public ActivityContainer()
        {
            Init();
        }

#endif

        private void Init()
        {
            _activityStackManager.BackStackChanged += ActivityStackManagerOnBackStackChanged;
#if WINDOWS_UWP
            UWPInit();
#endif
        }

#if ANDROID
        public bool IsHitTestVisible { get; set; }
#endif

        public bool DisableCache { get; set; }
        private ActivityModel CurrentActivityModel => _activityStackManager?.CurrentActivity;
        public Activity CurrentActivity => _activityStackManager?.CurrentActivity?.GetActivity(this);
        public bool CanGoBack => _activityStackManager.CanGoBack;
        public int BackStackDepth => _activityStackManager.BackStackDepth;
        public bool IsNavigating { get; private set; }
        public IActivityTransition ActivityTransition { get; set; }

        private IActivityTransition ActualActivityTransition =>
            CurrentActivity?.ActivityTransition ?? ActivityTransition;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs> Navigated;
        public event EventHandler<EventArgs> Navigating;
        public event EventHandler BackStackChanged;

        private void ActivityStackManagerOnBackStackChanged()
        {
            BackStackChanged?.Invoke(this, EventArgs.Empty);
        }

        private Task<bool> NavigateWithMode(ActivityModel newActivity, NavigationMode navigationMode)
        {
            return RunNavigationWithCheck(async () =>
            {
                var currentActivity = CurrentActivityModel;

                _activityStackManager.ClearForwardStack();

                await NavigateImplAsync(navigationMode, currentActivity, newActivity,
                    _activityStackManager.CurrentIndex + 1);

                return true;
            });
        }

        private async Task<bool> RunNavigationWithCheck(Func<Task<bool>> task)
        {
            if (IsNavigating)
            {
                return false;
            }

            try
            {
                IsNavigating = true;
                return await task();
            }
            finally
            {
                IsNavigating = false;
            }
        }

        public void OnBackPressed()
        {
            CurrentActivity?.OnBackPressed();
        }

        public void ClearBackStack()
        {
            _activityStackManager.ClearBackStack();
        }

        public void Navigate(Type type)
        {
        }

        public Task<bool> Navigate(Type activityType, object parameter)
        {
            var newActivity = new ActivityModel(activityType, parameter);
            return NavigateWithMode(newActivity, NavigationMode.New);
        }

        public Task<bool> Navigate<T>(object parameter = null) where T : Activity
        {
            return Navigate(typeof(T), parameter);
        }

        protected virtual void OnCurrentActivityChanged(Activity currentActivity, Activity newActivity)
        {
        }

        public Task<bool> GoBack()
        {
            return RunNavigationWithCheck(async () =>
            {
                await GoForwardOrBack(NavigationMode.Back);
                return true;
            });
        }

#if ANDROID
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            return !IsHitTestVisible || base.DispatchTouchEvent(e);
        }
#endif

        internal void FinishActivity(Activity activity)
        {
            if (CurrentActivity == activity)
            {
                if (CanGoBack)
                {
                    GoBack();
                }
            }
            else
            {
                _activityStackManager.RemoveActivity(activity);
            }
        }

        private void InsertChild(int position, Activity activity)
        {
#if ANDROID
            AddView(activity, position);
#elif WINDOWS_UWP
            Children.Insert(position, activity);
#endif
        }

        private void AddChild(Activity activity)
        {
#if ANDROID
            AddView(activity);
#elif WINDOWS_UWP
            Children.Add(activity);
#endif
        }

        private void RemoveChild(Activity activity)
        {
#if ANDROID
            RemoveView(activity);
#elif WINDOWS_UWP
            Children.Remove(activity);
#endif
        }

        public void ClearChild()
        {
#if ANDROID
            if (ChildCount > 0)
            {
                RemoveAllViews();
            }
#elif WINDOWS_UWP
            if (Children.Any())
            {
                Children.Clear();
            }
#endif
        }

        private async Task GoForwardOrBack(NavigationMode navigationMode)
        {
            if (CanGoBack)
            {
                var currentActivity = CurrentActivityModel;
                var nextActivityIndex =
                    _activityStackManager.CurrentIndex - 1;
                var nextActivity = _activityStackManager.Activities[nextActivityIndex];

                await NavigateImplAsync(navigationMode, currentActivity, nextActivity, nextActivityIndex);

                _activityStackManager.ClearForwardStack();
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(ActivityContainer)} cannot go back");
            }
        }

        private async Task NavigateImplAsync(NavigationMode navigationMode,
            ActivityModel currentActivity, ActivityModel nextActivity, int nextIndex)
        {
            IsHitTestVisible = false;

            InvokeLifecycleBeforeContentChanged(navigationMode, currentActivity, nextActivity);

            _activityStackManager.ChangeCurrentActivity(nextActivity, nextIndex);

            OnCurrentActivityChanged(currentActivity?.Activity, nextActivity?.Activity);

            Navigating?.Invoke(this, EventArgs.Empty);

            await UpdateContent(navigationMode, currentActivity, nextActivity);

            InvokeLifecycleAfterContentChanged(navigationMode, currentActivity, nextActivity);

            IsHitTestVisible = true;

            ReleaseActivity(currentActivity);

            OnNotifyPropertyChanged(nameof(CanGoBack));

            Navigated?.Invoke(this, EventArgs.Empty);
        }


        private void AddActivityToContentRoot(NavigationMode navigationMode, ActivityInsertionMode insertionMode,
            Activity next)
        {
            switch (navigationMode)
            {
                case NavigationMode.New when insertionMode == ActivityInsertionMode.NewAbove:
                case NavigationMode.Back when insertionMode == ActivityInsertionMode.NewBelow:
                    AddChild(next);
                    break;
                case NavigationMode.Back when insertionMode == ActivityInsertionMode.NewAbove:
                case NavigationMode.New when insertionMode == ActivityInsertionMode.NewBelow:
                    InsertChild(0, next);
                    break;
            }
        }

        private async Task UpdateContent(NavigationMode navigationMode, ActivityModel currentActivity,
            ActivityModel nextActivity)
        {
            var animation = ActualActivityTransition;
            var current = currentActivity?.GetActivity(this);
            var next = nextActivity?.GetActivity(this);
#if WINDOWS_UWP
            currentActivity?.GetActivity(this)?.PrepareConnectedAnimation();
#endif
            if (animation != null)
            {
                AddActivityToContentRoot(navigationMode, animation.InsertionMode, next);
#if WINDOWS_UWP
                nextActivity?.GetActivity(this)?.UsingConnectedAnimation();
#endif
                switch (navigationMode)
                {
                    case NavigationMode.New:
                        await animation.OnStart(next, current);
                        break;
                    case NavigationMode.Back:
                        await animation.OnClose(current, next);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(navigationMode), navigationMode, null);
                }

                RemoveChild(current);
            }
            else
            {
                ClearChild();
                AddChild(next);
#if WINDOWS_UWP
                nextActivity?.GetActivity(this)?.UsingConnectedAnimation();
#endif
            }
        }

        private void InvokeLifecycleBeforeContentChanged(NavigationMode navigationMode, ActivityModel currentActivity,
            ActivityModel nextActivity)
        {
            switch (navigationMode)
            {
                case NavigationMode.New:
                    currentActivity?.GetActivity(this)?.OnPause();
                    nextActivity?.GetActivity(this)?.OnStart();
                    break;
                case NavigationMode.Back:
                    currentActivity?.GetActivity(this)?.OnClose();
                    nextActivity?.GetActivity(this).OnRestart();
                    break;
                case NavigationMode.Forward:
                    break;
                case NavigationMode.Refresh:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(navigationMode), navigationMode, null);
            }
        }

        private void InvokeLifecycleAfterContentChanged(NavigationMode navigationMode, ActivityModel currentActivity,
            ActivityModel nextActivity)
        {
            switch (navigationMode)
            {
                case NavigationMode.New:
                    currentActivity?.GetActivity(this)?.OnStop();
                    nextActivity?.GetActivity(this)?.OnResume();
                    break;
                case NavigationMode.Back:
                    currentActivity?.GetActivity(this)?.OnDestroy();
                    nextActivity?.GetActivity(this).OnResume();
                    break;
                case NavigationMode.Forward:
                    break;
                case NavigationMode.Refresh:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(navigationMode), navigationMode, null);
            }
        }

        private void ReleaseActivity(ActivityModel activity)
        {
            if (activity != null &&
                (activity.Activity.NavigationCacheMode == NavigationCacheMode.Disabled || DisableCache))
            {
                activity.Release();
            }
        }

        protected virtual void OnNotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public enum NavigationMode
    {
        New,
        Back,
        Forward,
        Refresh
    }

    public enum NavigationCacheMode
    {
        Disabled,
        Required,
        Enabled
    }
}