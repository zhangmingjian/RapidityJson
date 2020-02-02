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

        public static JsonArray Create(string json)
        {
            return new JsonParser().ParseArray(json);
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

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
