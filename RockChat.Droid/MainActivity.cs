using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using RockChat.Controls.Paging;

namespace RockChat.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ActivityContainer _rootActivityContainer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _rootActivityContainer = new ActivityContainer(this);
            SetContentView(_rootActivityContainer);
        }

        public override void OnBackPressed()
        {
            if (_rootActivityContainer.CanGoBack)
            {
                _rootActivityContainer.OnBackPressed();
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}