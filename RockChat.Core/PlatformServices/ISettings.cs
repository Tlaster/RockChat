using System;
using System.Collections.Generic;
using System.Text;

namespace RockChat.Core.PlatformServices
{
    public interface ISettings
    {
        T Get<T>(string key, T defaultValue = default);
        void Set<T>(string key, T value);
    }
}
