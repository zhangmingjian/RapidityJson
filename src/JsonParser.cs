using System;
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
            using (var read = new JsonReader(json))
            {
                return new JsonSerializer().Deserialize<T>(read);
            }
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
            return new JsonSerializer().Serialize(obj);
        }

        public static string ToJson(object obj, bool indented)
        {
            var option = JsonOption.Defalut;
            option.Indented = indented;
            return new JsonSerializer(option).Serialize(obj);
        }

        public static string ToJson(object obj, JsonOption option)
        {
            return new JsonSerializer(option).Serialize(obj);
        }
    }
}
