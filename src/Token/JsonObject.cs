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
            return new Dictionary<string, JsonToken>.Enumerator();
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