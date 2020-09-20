using System;
using System.Collections;
using System.Collections.Generic;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonArray : JsonElement, IEnumerable<JsonElement>
    {
        private readonly List<JsonElement> _list;

        public override JsonElementType ElementType => JsonElementType.Array;

        public JsonArray() : this(new JsonElement[0])
        {
        }

        public JsonArray(IEnumerable<JsonElement> elements)
        {
            _list = new List<JsonElement>(elements) { Capacity = 8 };
        }

        public int Count => _list.Count;

        public JsonElement this[int index]
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

        public void Add(JsonElement token)
        {
            _list.Add(token ?? new JsonNull());
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public bool Remove(JsonElement token)
        {
            return _list.Remove(token);
        }

        public int Remove(Predicate<JsonElement> match)
        {
            return _list.RemoveAll(match);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator<JsonElement> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// [start:end]数组片段，区间为[start,end),不包含end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public JsonArray GetArray(int start, int end)
        {
            var arr = new JsonArray();
            if (start < this.Count && end > start) 
                arr._list.AddRange(this._list.GetRange(start, Math.Min(end - start, this.Count - start)));
            return arr;
        }

        public JsonObject GetObject(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ElementType)
            {
                case JsonElementType.Object: return (JsonObject)token;
                case JsonElementType.Null: return null;
                default: throw new Exception($"类型:{token.ElementType}不支持转换为JsonObject");
            }
        }

        public JsonArray GetArray(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ElementType)
            {
                case JsonElementType.Array: return (JsonArray)token;
                case JsonElementType.Null: return null;
                default: throw new Exception($"类型:{token.ElementType}不支持转换为JsonArray");
            }
        }

        public string GetString(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var token = this[index];
            switch (token.ElementType)
            {
                case JsonElementType.String: return ((JsonString)token).Value;
                case JsonElementType.Null: return null;
                case JsonElementType.Number: return token.ToString();
                default: throw new Exception($"类型:{token.ElementType}不支持转换为String");
            }
        }

        public int? GetInt(int index)
        {
            if (index < 0 || index > this.Count - 1) return null;
            var element = this[index];
            switch (element.ElementType)
            {
                case JsonElementType.String:
                    var value = ((JsonString)element).Value;
                    if (int.TryParse(value, out int v)) return v;
                    else new Exception($"JsonString:{value}不是正确的int类型");
                    return null;
                case JsonElementType.Null: return null;
                case JsonElementType.Number:
                    var jsonNum = (JsonNumber)element;
                    if (jsonNum.TryGetInt(out int intV)) return intV;
                    return null;
                default: throw new Exception($"类型:{element.ElementType}不支持转换为String");
            }
        }

    }
}
