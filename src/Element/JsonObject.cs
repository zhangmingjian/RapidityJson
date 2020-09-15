using System;
using System.Collections;
using System.Collections.Generic;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonObject : JsonElement, IEnumerable<KeyValuePair<string, JsonElement>>
    {
        private readonly Dictionary<string, JsonElement> _dictionary;
        public override JsonElementType ElementType => JsonElementType.Object;

        public JsonObject()
        {
            _dictionary = new Dictionary<string, JsonElement>();
        }

        public JsonElement this[string propertyName]
        {
            get => GetValue(propertyName);
            set => AddProperty(propertyName, value);
        }

        public IEnumerable<JsonProperty> GetAllProperty()
        {
            if (_dictionary.Count == 0) yield return default;
            foreach (var key in _dictionary.Keys)
                yield return new JsonProperty(key, _dictionary[key]);
        }

        public IReadOnlyCollection<string> GetPropertyNames() => _dictionary.Keys;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public new static JsonObject Parse(string json)
        {
            return Parse(json, new JsonOption());
        }

        public new static JsonObject Parse(string json, JsonOption option)
        {
            using (var reader = new JsonReader(json))
            {
                return new JsonSerializer(option).Deserialize<JsonObject>(reader);
            }
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(JsonProperty property)
        {
            AddProperty(property.Name, property.Value);
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddProperty(string name, JsonElement value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _dictionary[name] = value ?? new JsonNull();
        }

        public JsonElement GetValue(string property)
        {
            if (TryGetValue(property, out JsonElement value))
                return value;
            throw new JsonException($"属性：{property}不存在");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool TryGetValue(string property, out JsonElement element)
        {
            if (property == null)
            {
                element = null;
                return false;
            }
            return _dictionary.TryGetValue(property, out element);
        }

        public bool ContainsProperty(string property)
        {
            return _dictionary.ContainsKey(property);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool Remove(string property)
        {
            return _dictionary.Remove(property);
        }

        public IEnumerator<KeyValuePair<string, JsonElement>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public JsonObject GetObject(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Object: return (JsonObject)element;
                case JsonElementType.Null: return null;
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为JsonObject");
            }
        }

        public JsonArray GetArray(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Array: return (JsonArray)element;
                case JsonElementType.Null: return null;
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为JsonArray");
            }
        }

        public string GetString(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String:
                case JsonElementType.Null:
                case JsonElementType.Number: return element.ToString();
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为String");
            }
        }

        public bool? GetBoolean(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.Boolean: return ((JsonBoolean)element).Value;
                case JsonElementType.Null: return null;
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为Boolean");
            }
        }

        public int? GetInt(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return int.TryParse(((JsonString)element).Value, out int val) ? val : default(int?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetInt(out int val1) ? val1 : default(int?);
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为int");
            }
        }

        public long? GetLong(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return long.TryParse(((JsonString)element).Value, out long val) ? val : default(long?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetLong(out long val1) ? val1 : default(long?);
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转换为long");
            }
        }

        public float? GetFloat(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return float.TryParse(((JsonString)element).Value, out float val) ? val : default(float?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetFloat(out float val1) ? val1 : default(float?);
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转化为float");
            }
        }

        public double? GetDouble(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return double.TryParse(((JsonString)element).Value, out double val) ? val : default(double?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetDouble(out double val1) ? val1 : default(double?);
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转化为double");
            }
        }

        public decimal? GetDecimal(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return decimal.TryParse(((JsonString)element).Value, out decimal val) ? val : default(decimal?);
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return ((JsonNumber)element).TryGetDecimal(out decimal val1) ? val1 : default(decimal?);
                default: throw new Exception($"属性{property}:{element.ElementType}不转化为decimal");
            }
        }

        public DateTime? GetDateTime(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return DateTime.TryParse(((JsonString)element).Value, out DateTime val) ? val : default(DateTime?);
                case JsonElementType.Null: return null;
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转化为DateTime");
            }
        }

        public Guid? GetGuid(string property)
        {
            var element = this[property];
            if (element == null) return null;
            switch (element.ElementType)
            {
                case JsonElementType.String: return Guid.TryParse(((JsonString)element).Value, out Guid val) ? val : default(Guid?);
                case JsonElementType.Null: return null;
                default: throw new Exception($"属性{property}:{element.ElementType}不支持转化为Guid");
            }
        }

    }

    /// <summary>
    /// 属性对象
    /// </summary>
    public struct JsonProperty
    {
        private string _name;
        private JsonElement _value;

        public string Name => _name;
        public JsonElement Value => _value;
        public JsonProperty(string name, JsonElement value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _name = name;
            _value = value ?? new JsonNull();
        }
    }
}