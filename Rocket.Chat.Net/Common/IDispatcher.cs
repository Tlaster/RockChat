using System;

namespace Rocket.Chat.Net.Common
{
    public interface IDispatcher
    {
        void RunOnMainThread(Action action);
    }
}