using System;
using System.Collections;
using System.Collections.Generic;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonArray : JsonToken, IEnumerable<JsonToken>
    {
        private readonly List<JsonToken> _list;

        public override JsonValueType ValueType => JsonValueType.Array;

        public JsonArray()
        {
            _list = new List<JsonToken>();
        }

        public int Count => _list.Count;

        public JsonToken this[int index]
        {
            get => _list[index];
            set => _list[index] = value ?? new JsonNull();
        }

        public new static JsonArray Parse(string json)
        {
            return Parse(json, new JsonOption());
        }

        public new static JsonArray Parse(string json, JsonOption option)
        {
            using (var reader = new JsonReader(json))
            {
                return new JsonSerializer(option).Deserialize<JsonArray>(reader);
            }
        }

        public void Add(JsonToken token)
        {
            _list.Add(token ?? new JsonNull());
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public bool Remove(JsonToken token)
        {
            return _list.Remove(token);
        }

        public int Remove(Predicate<JsonToken> match)
        {
            return _list.RemoveAll(match);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator<JsonToken> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public JsonObject GetObject(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ValueType)
            {
                case JsonValueType.Object: return (JsonObject)token;
                case JsonValueType.Null: return null;
                default: throw new Exception($"类型:{token.ValueType}不支持转换为JsonObject");
            }
        }

        public JsonArray GetArray(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ValueType)
            {
                case JsonValueType.Array: return (JsonArray)token;
                case JsonValueType.Null: return null;
                default: throw new Exception($"类型:{token.ValueType}不支持转换为JsonArray");
            }
        }

        public string GetString(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ValueType)
            {
                case JsonValueType.String: return ((JsonString)token).Value;
                case JsonValueType.Null: return null;
                case JsonValueType.Number: return token.ToString();
                default: throw new Exception($"类型:{token.ValueType}不支持转换为String");
            }
        }

        public int? GetInt(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ValueType)
            {
                case JsonValueType.String:
                    var value = ((JsonString)token).Value;
                    if (int.TryParse(value, out int v)) return v;
                    else new Exception($"JsonString:{value}不是正确的int类型");
                    return null;
                case JsonValueType.Null: return null;
                case JsonValueType.Number:
                    var jsonNum = (JsonNumber)token;
                    if (jsonNum.TryGetInt(out int intV)) return intV;
                    return null;
                default: throw new Exception($"类型:{token.ValueType}不支持转换为String");
            }
        }
    }
}
