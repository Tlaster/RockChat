using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.Helpers;
using RockChat.Core.PlatformServices;

namespace RockChat.UWP.PlatformServices
{
    class Settings : ISettings
    {
        private readonly LocalObjectStorageHelper _helper = new LocalObjectStorageHelper();
        public T Get<T>(string key, T defaultValue = default)
        {
            return _helper.Read(key, defaultValue);
        }

        public void Set<T>(string key, T value)
        {
            _helper.Save(key, value);
        }
    }
}
