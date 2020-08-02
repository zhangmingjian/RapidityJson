using System;
using System.Collections;
using System.Collections.Generic;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonObject : JsonToken, IEnumerable<KeyValuePair<string, JsonToken>>
    {
        private readonly Dictionary<string, JsonToken> _dictionary;
        public override JsonValueType ValueType => JsonValueType.Object;

        public JsonObject()
        {
            _dictionary = new Dictionary<string, JsonToken>();
        }

        public JsonToken this[string propertyName]
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
        public void AddProperty(string name, JsonToken value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _dictionary[name] = value ?? new JsonNull();
        }

        public JsonToken GetValue(string property)
        {
            if (TryGetValue(property, out JsonToken value))
                return value;
            throw new JsonException($"属性：{property}不存在");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool TryGetValue(string property, out JsonToken token)
        {
            if (property == null)
            {
                token = null;
                return false;
            }
            return _dictionary.TryGetValue(property, out token);
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

        public IEnumerator<KeyValuePair<string, JsonToken>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public JsonObject GetObject(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.Object: return (JsonObject)token;
                case JsonValueType.Null: return null;
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为JsonObject");
            }
        }

        public JsonArray GetArray(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.Array: return (JsonArray)token;
                case JsonValueType.Null: return null;
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为JsonArray");
            }
        }

        public string GetString(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String:
                case JsonValueType.Null:
                case JsonValueType.Number: return token.ToString();
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为String");
            }
        }

        public bool? GetBoolean(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.Boolean: return ((JsonBoolean)token).Value;
                case JsonValueType.Null: return null;
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为Boolean");
            }
        }

        public int? GetInt(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return int.TryParse(((JsonString)token).Value, out int val) ? val : default(int?);
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return ((JsonNumber)token).TryGetInt(out int val1) ? val1 : default(int?);
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为int");
            }
        }

        public long? GetLong(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return long.TryParse(((JsonString)token).Value, out long val) ? val : default(long?);
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return ((JsonNumber)token).TryGetLong(out long val1) ? val1 : default(long?);
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转换为long");
            }
        }

        public float? GetFloat(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return float.TryParse(((JsonString)token).Value, out float val) ? val : default(float?);
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return ((JsonNumber)token).TryGetFloat(out float val1) ? val1 : default(float?);
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转化为float");
            }
        }

        public double? GetDouble(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return double.TryParse(((JsonString)token).Value, out double val) ? val : default(double?);
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return ((JsonNumber)token).TryGetDouble(out double val1) ? val1 : default(double?);
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转化为double");
            }
        }

        public decimal? GetDecimal(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return decimal.TryParse(((JsonString)token).Value, out decimal val) ? val : default(decimal?);
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return ((JsonNumber)token).TryGetDecimal(out decimal val1) ? val1 : default(decimal?);
                default: throw new Exception($"属性{property}:{token.ValueType}不转化为decimal");
            }
        }

        public DateTime? GetDateTime(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return DateTime.TryParse(((JsonString)token).Value, out DateTime val) ? val : default(DateTime?);
                case JsonValueType.Null: return null;
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转化为DateTime");
            }
        }

        public Guid? GetGuid(string property)
        {
            var token = this[property];
            if (token == null) return null;
            switch (token.ValueType)
            {
                case JsonValueType.String: return Guid.TryParse(((JsonString)token).Value, out Guid val) ? val : default(Guid?);
                case JsonValueType.Null: return null;
                default: throw new Exception($"属性{property}:{token.ValueType}不支持转化为Guid");
            }
        }

    }

    /// <summary>
    /// 属性对象
    /// </summary>
    public struct JsonProperty
    {
        private string _name;
        private JsonToken _value;

        public string Name => _name;
        public JsonToken Value => _value;
        public JsonProperty(string name, JsonToken value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _name = name;
            _value = value ?? new JsonNull();
        }
    }
}