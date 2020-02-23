using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Animations;
using RockChat.Controls.Paging;

namespace RockChat.UWP.Common
{
    internal class FadeActivityTransition : IActivityTransition
    {
        private readonly double _duration = 250d;
        public ActivityInsertionMode InsertionMode { get; } = ActivityInsertionMode.NewBelow;

        public async Task OnStart(Activity newActivity, Activity currentActivity)
        {
            if (currentActivity != null)
            {
                currentActivity.Opacity = 1f;
                currentActivity.Fade(duration: _duration).Start();
            }

            if (newActivity != null)
            {
                newActivity.Opacity = 0f;
                await newActivity.Fade(1, duration: _duration).StartAsync();
            }
        }

        public async Task OnClose(Activity closeActivity, Activity previousActivity)
        {
            if (closeActivity != null)
            {
                closeActivity.Opacity = 1f;
                closeActivity.Fade(duration: _duration).Start();
            }

            if (previousActivity != null)
            {
                previousActivity.Opacity = 0f;
                await previousActivity.Fade(1, duration: _duration).StartAsync();
            }
        }
    }
}