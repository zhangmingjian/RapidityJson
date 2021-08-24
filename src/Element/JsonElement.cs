using Rapidity.Json.JsonPath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rapidity.Json
{
    /// <summary>
    /// JsonElement
    /// </summary>
    public abstract class JsonElement
    {
        public abstract JsonElementType ElementType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonElement Create(string json)
        {
            return Create(json, new JsonOption());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonElement Create(string json, JsonOption option)
        {
            using (var reader = new JsonReader(json))
            {
                return new JsonSerializer(option).Deserialize<JsonElement>(reader);
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="jsonPath"></param>
        ///// <returns></returns>
        //public JsonElement Filter(string jsonPath)
        //{
        //    return Filters(jsonPath)?.FirstOrDefault();
        //}

        ///// <summary>
        ///// 匹配多个
        ///// </summary>
        ///// <param name="jsonPath"></param>
        ///// <returns></returns>
        //public IEnumerable<JsonElement> Filters(string jsonPath)
        //{
        //    var filters = new DefaultJsonPathResolver().Resolve(jsonPath);
        //    IEnumerable<JsonElement> current = null;
        //    foreach (var filter in filters)
        //    {
        //        current = filter.Filter(this, current);
        //        if (current == null || current.Count() == 0) break;
        //    }
        //    return current;
        //}

        public JsonElement Get(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;
            JsonElement target = this;
            foreach (var name in path.Split('/'))
            {
                switch (target.ElementType)
                {
                    case JsonElementType.Object:
                        target = ((JsonObject)target).TryGetValue(name);
                        break;
                    case JsonElementType.Array:
                        if (int.TryParse(name, out int index))
                            target = ((JsonArray)target)[index];
                        break;
                    default: target = null; break;
                }
                if (target == null) break;
            }
            return target;
        }

        public JsonObject GetObject(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Object: return (JsonObject)element;
                case JsonElementType.Null: return null;
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为JsonObject");
            }
        }

        public JsonArray GetArray(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Array: return (JsonArray)element;
                case JsonElementType.Null: return null;
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为JsonArray");
            }
        }

        public string GetString(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String:
                case JsonElementType.Boolean:
                case JsonElementType.Null:
                case JsonElementType.Number: return element.ToString();
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为String");
            }
        }

        public bool? TryGetBool(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Boolean: return ((JsonBoolean)element).Value;
                case JsonElementType.String: return bool.TryParse(((JsonString)element).Value, out bool b) ? b : default;
                case JsonElementType.Null: return null;
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为Boolean");
            }
        }

        public bool GetBool(string path) => TryGetBool(path) ?? false;

        public int? TryGetInt(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return int.TryParse(((JsonString)element).Value, out int val) ? val : default(int?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetInt(out int val1) ? val1 : default(int?);
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为int");
            }
        }

        public int GetInt(string path) => TryGetInt(path) ?? 0;

        public long? TryGetLong(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return long.TryParse(((JsonString)element).Value, out long val) ? val : default(long?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetLong(out long val1) ? val1 : default(long?);
                default: throw new Exception($"path{path}:{element.ElementType}不支持转换为long");
            }
        }

        public long GetLong(string path) => TryGetLong(path) ?? 0L;

        public float? TryGetFloat(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return float.TryParse(((JsonString)element).Value, out float val) ? val : default(float?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetFloat(out float val1) ? val1 : default(float?);
                default: throw new Exception($"path{path}:{element.ElementType}不支持转化为float");
            }
        }

        public float GetFloat(string path) => TryGetFloat(path) ?? 0f;

        public double? TryGetDouble(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return double.TryParse(((JsonString)element).Value, out double val) ? val : default(double?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetDouble(out double val1) ? val1 : default(double?);
                default: throw new Exception($"path{path}:{element.ElementType}不支持转化为double");
            }
        }

        public double GetDouble(string path) => TryGetDouble(path) ?? 0d;

        public decimal? TryGetDecimal(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return decimal.TryParse(((JsonString)element).Value, out decimal val) ? val : default(decimal?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetDecimal(out decimal val1) ? val1 : default(decimal?);
                default: throw new Exception($"path{path}:{element.ElementType}不转化为decimal");
            }
        }

        public decimal GetDecimal(string path) => TryGetDecimal(path) ?? 0m;

        public DateTime? TryGetTime(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return DateTime.TryParse(((JsonString)element).Value, out DateTime val) ? val : default(DateTime?);
                case JsonElementType.Null: return null;
                default: throw new Exception($"path{path}:{element.ElementType}不支持转化为DateTime");
            }
        }

        public DateTime GetTime(string path) => TryGetTime(path) ?? DateTime.MinValue;

        public Guid? TryGetGuid(string path)
        {
            var element = Get(path);
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return Guid.TryParse(((JsonString)element).Value, out Guid val) ? val : default(Guid?);
                case JsonElementType.Null: return null;
                default: throw new Exception($"path{path}:{element.ElementType}不支持转化为Guid");
            }
        }

        public Guid GetGuid(string property) => TryGetGuid(property) ?? Guid.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object To(Type type)
        {
            return new JsonSerializer().Deserialize(this, type);
        }

        public virtual object To(Type type, JsonOption option)
        {
            return new JsonSerializer(option).Deserialize(this, type);
        }

        public virtual T To<T>()
        {
            return To<T>(new JsonOption());
        }

        public virtual T To<T>(JsonOption option)
        {
            return (T)To(typeof(T), option);
        }

        public override string ToString()
        {
            var option = new JsonOption
            {
                SkipValidated = true
            };
            return ToString(option);
        }

        public virtual string ToString(JsonOption option)
        {
            return new JsonSerializer(option).Serialize(this);
        }

        #region 基本类型转换

        public static implicit operator JsonElement(string value) => new JsonString(value);
        public static implicit operator JsonElement(bool value) => new JsonBoolean(value);
        public static implicit operator JsonElement(byte value) => new JsonNumber(value);
        public static implicit operator JsonElement(short value) => new JsonNumber(value);
        public static implicit operator JsonElement(int value) => new JsonNumber(value);
        public static implicit operator JsonElement(long value) => new JsonNumber(value);
        public static implicit operator JsonElement(float value) => new JsonNumber(value);
        public static implicit operator JsonElement(double value) => new JsonNumber(value);
        public static implicit operator JsonElement(decimal value) => new JsonNumber(value);

        #endregion
    }
}