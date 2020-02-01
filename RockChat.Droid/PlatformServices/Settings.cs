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
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using RockChat.Core.PlatformServices;

namespace RockChat.Droid.PlatformServices
{
    class Settings : ISettings
    {
        private readonly Context _appContext;

        public Settings(Context applicationContext)
        {
            _appContext = applicationContext;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            var sharedPreferences = GetSharedPreferences();
            object result = defaultValue switch
            {
                string strValue => sharedPreferences.GetString(key, strValue),
                int intValue => sharedPreferences.GetInt(key, intValue),
                float floatValue => sharedPreferences.GetFloat(key, floatValue),
                long longValue => sharedPreferences.GetLong(key, longValue),
                bool boolValue => sharedPreferences.GetBoolean(key, boolValue),
                _ => JsonConvert.DeserializeObject<T>(sharedPreferences.GetString(key, string.Empty))
            };

            if (result is T tresult)
            {
                return tresult;
            }

            return defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            var sharedPreferences = GetSharedPreferences().Edit();
            switch (value)
            {
                case string strValue:
                    sharedPreferences.PutString(key, strValue);
                    break;
                case int intValue:
                    sharedPreferences.PutInt(key, intValue);
                    break;
                case float floatValue:
                    sharedPreferences.PutFloat(key, floatValue);
                    break;
                case long longValue:
                    sharedPreferences.PutLong(key, longValue);
                    break;
                case bool boolValue:
                    sharedPreferences.PutBoolean(key, boolValue);
                    break;
                default:
                    sharedPreferences.PutString(key, JsonConvert.SerializeObject(value));
                    break;
            }
            sharedPreferences.Apply();
        }

        private ISharedPreferences GetSharedPreferences()
        {
            return _appContext.GetSharedPreferences("RockChat", FileCreationMode.Private);
        }
    }
}