using System.Collections.Generic;

namespace Rocket.Chat.Net.Models
{
    interface IMethodCall<T>
    {
        string Method { get; }
        List<T> Params { get; }
    }
}