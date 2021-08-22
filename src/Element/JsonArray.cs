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

        public JsonArray()
        {
            _list = new List<JsonElement>();
        }

        public JsonArray(IEnumerable<JsonElement> elements)
        {
            _list = new List<JsonElement>(elements);
        }

        public int Count => _list.Count;

        public JsonElement this[int index]
        {
            get => index >= 0 && index < _list.Count ? _list[index] : null;
            set
            {
                if (index >= 0 && index < _list.Count)
                    _list[index] = value ?? new JsonNull();
            }
        }

        public new static JsonArray Create(string json)
        {
            return Create(json, new JsonOption());
        }

        public new static JsonArray Create(string json, JsonOption option)
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
        /// 数组切片操作
        /// [start:end:step]数组片段，区间为[start,end),不包含end,step步长
        /// </summary>
        /// <param name="strat"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public JsonArray Slice(int start, int end, int step = 1)
        {
            start = Math.Max(0, start);
            end = Math.Min(this.Count, end);
            step = Math.Max(1, step);
            var arr = new JsonArray();
            for (int i = start; i < end; i += step)
            {
                arr.Add(this[i]);
            }
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
