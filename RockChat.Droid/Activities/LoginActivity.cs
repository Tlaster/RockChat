using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using RockChat.Controls.Paging;

namespace RockChat.Droid.Activities
{
    class LoginActivity : Activity
    {
        protected LoginActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LoginActivity(Context context) : base(context)
        {
        }

        public LoginActivity(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public LoginActivity(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public LoginActivity(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

    }
}