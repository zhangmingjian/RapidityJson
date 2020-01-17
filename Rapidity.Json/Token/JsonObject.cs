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
        private readonly Dictionary<string, JsonToken> _store;
        public override JsonValueType ValueType => JsonValueType.Object;

        public JsonObject()
        {
            _store = new Dictionary<string, JsonToken>();
        }

        public JsonToken this[string propertyName]
        {
            get => GetValue(propertyName);
            set => AddProperty(propertyName, value);
        }

        public IEnumerable<JsonProperty> GetAllProperty()
        {
            if (_store.Count == 0) yield return default;
            foreach (var key in _store.Keys)
                yield return new JsonProperty(key, _store[key]);
        }

        public IReadOnlyCollection<string> GetPropertyNames() => _store.Keys;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonObject Create(string json)
        {
            return new JsonParser().ParseObject(json);
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
            _store[name] = value ?? new JsonNull();
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
            return _store.TryGetValue(property, out token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool Remove(string property)
        {
            return _store.Remove(property);
        }

        public IEnumerator<KeyValuePair<string, JsonToken>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Dictionary<string, JsonToken>.Enumerator();
        }

        public override object To(Type type)
        {
            throw new NotImplementedException();
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