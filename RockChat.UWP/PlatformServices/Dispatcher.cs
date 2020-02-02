using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Rocket.Chat.Net.Common;

namespace RockChat.UWP.PlatformServices
{
    public class Dispatcher : IDispatcher
    {
        public void RunOnMainThread(Action action)
        {
            DispatcherHelper.ExecuteOnUIThreadAsync(action.Invoke);
        }
    }
}
