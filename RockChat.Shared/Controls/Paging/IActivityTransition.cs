using System;
using System.Threading.Tasks;

namespace RockChat.Controls.Paging
{
    /// <summary>Enumeration of the possible activity insertion modes. </summary>
    public enum ActivityInsertionMode
    {
        /// <summary>
        ///     Inserts the new activity over the previous activity before starting the animations so that both activities are
        ///     in the visual tree during the animations.
        /// </summary>
        NewAbove,

        /// <summary>
        ///     Inserts the new activity below the previous activity before starting the animations so that both activities
        ///     are in the visual tree during the animations.
        /// </summary>
        NewBelow
    }

    public interface IActivityTransition
    {
        ActivityInsertionMode InsertionMode { get; }

        Task OnStart(Activity newActivity, Activity currentActivity);

        Task OnClose(Activity closeActivity, Activity previousActivity);
    }

    //internal class DefaultActivityTransition : IActivityTransition
    //{
    //    public ActivityInsertionMode InsertionMode { get; } = ActivityInsertionMode.NewBelow;

    //    public async Task OnStart(Activity newActivity, Activity currentActivity)
    //    {
    //        newActivity?.BeginAnimation(UIElement.OpacityProperty,
    //            new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromSeconds(0.25))));
    //        currentActivity?.BeginAnimation(UIElement.OpacityProperty,
    //            new DoubleAnimation(1d, 0d, new Duration(TimeSpan.FromSeconds(0.25))));
    //    }

    //    public async Task OnClose(Activity closeActivity, Activity previousActivity)
    //    {
    //        closeActivity?.BeginAnimation(UIElement.OpacityProperty,
    //            new DoubleAnimation(1d, 0d, new Duration(TimeSpan.FromSeconds(0.25))));
    //        previousActivity?.BeginAnimation(UIElement.OpacityProperty,
    //            new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromSeconds(0.25))));
    //    }
    //}
}