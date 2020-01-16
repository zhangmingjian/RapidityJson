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
        private readonly List<JsonToken> _store;

        public override JsonValueType ValueType => JsonValueType.Array;

        public JsonArray()
        {
            _store = new List<JsonToken>();
        }

        public int Count => _store.Count;

        public JsonToken this[int index]
        {
            get => _store[index];
            set => _store[index] = value ?? new JsonNull();
        }

        public static JsonArray Create(string json)
        {
            return new JsonParser().ParseArray(json);
        }

        public void Add(JsonToken token)
        {
            _store.Add(token ?? new JsonNull());
        }

        public void RemoveAt(int index)
        {
            _store.RemoveAt(index);
        }

        public bool Remove(JsonToken token)
        {
            return _store.Remove(token);
        }

        public int Remove(Predicate<JsonToken> match)
        {
            return _store.RemoveAll(match);
        }

        public void Clear()
        {
            _store.Clear();
        }

        public IEnumerator<JsonToken> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
