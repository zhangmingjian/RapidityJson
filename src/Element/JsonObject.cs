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
            _dictionary = new Dictionary<string, JsonElement>(StringComparer.CurrentCultureIgnoreCase);
        }

        public JsonElement this[string property]
        {
            get => TryGetValue(property);
            set => AddProperty(property, value);
        }

        public int Count => _dictionary.Count;

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
        public new static JsonObject Create(string json)
        {
            return Create(json, new JsonOption());
        }

        public new static JsonObject Create(string json, JsonOption option)
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
            var value = TryGetValue(property);
            return value != null ? value : throw new JsonException($"属性：{property}不存在");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public JsonElement? TryGetValue(string property)
        {
            if (property == null) return null;
            return _dictionary.TryGetValue(property, out JsonElement element) ? element : null;
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