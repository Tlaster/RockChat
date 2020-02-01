using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using RockChat.Core;
using RockChat.Core.PlatformServices;
using RockChat.Droid.PlatformServices;

namespace RockChat.Droid
{
    [Application]
    public class App : Application
    {
        protected App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public App()
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RockApp.Init(builder =>
            {
                builder.RegisterInstance(new Settings(ApplicationContext)).As<ISettings>();

            });
        }
    }
}