﻿using System;
using System.Reflection;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T To<T>(string json)
        {
            return To<T>(json, new JsonOption());
        }

        public static T To<T>(string json, JsonOption option)
        {
            using (var read = new JsonReader(json))
            {
                return new JsonSerializer(option).Deserialize<T>(read);
            }
        }

        public static string ToJson(object obj)
        {
            var option = new JsonOption
            {
                SkipValidated = true
            };
            return ToJson(obj, option);
        }

        public static string ToJson(object obj, bool indented)
        {
            var option = new JsonOption
            {
                Indented = indented,
                SkipValidated = true
            };
            return ToJson(obj, option);
        }

        public static string ToJson(object obj, JsonOption option)
        {
            return new JsonSerializer(option).Serialize(obj);
        }
    }
}
