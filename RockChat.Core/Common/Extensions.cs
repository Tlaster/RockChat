﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RockChat.Core.Common
{

    internal static class Extensions
    {
        // Kotlin: fun <T> T.also(block: (T) -> Unit): T
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        public static void FireAndForget(this Task task)
        {
        }

        public static T FromJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        // Kotlin: fun <T, R> T.let(block: (T) -> R): R
        public static R Let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }

        public static string ToJson<T>(this T value)
        {
            return JsonConvert.SerializeObject(value);
        }

    }
}
