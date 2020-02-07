using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    /// <summary>
    /// 
    /// </summary>
    internal class DictionaryConverter : TypeConverter
    {
        public Type KeyType { get; protected set; }

        public Type ValueType { get; protected set; }

        public DictionaryConverter(Type type, Type keyType, Type valueType, TypeConverterProvider provider) : base(type, provider)
        {
            this.KeyType = keyType;
            this.ValueType = valueType;
        }

        protected virtual string AddKeyValueMethodName => nameof(IDictionary.Add);

        private Action<object, object, object> _setKeyValue;
        public Action<object, object, object> SetKeyValue => _setKeyValue = _setKeyValue ?? BuildSetKeyValueMethod(Type);

        protected virtual Action<object, object, object> BuildSetKeyValueMethod(Type type)
        {
            var objExp = Expression.Parameter(typeof(object), "dic");
            var keyParaExp = Expression.Parameter(typeof(object), "key");
            var valueParaExp = Expression.Parameter(typeof(object), "value");
            var dicExp = Expression.TypeAs(objExp, Type);
            var keyExp = Expression.Convert(keyParaExp, KeyType);
            var valueExp = Expression.Convert(valueParaExp, ValueType);
            //调用索引器赋值
            var property = type.GetProperty("Item", new Type[] { KeyType });
            var indexExp = Expression.MakeIndex(dicExp, property, new Expression[] { keyExp });
            var body = Expression.Assign(indexExp, valueExp);
            //var method = type.GetMethod(AddKeyValueMethodName, new Type[] { KeyType, ValueType });
            //var body = Expression.Call(dicExp, method, keyExp, valueExp);
            var expression = Expression.Lambda<Action<object, object, object>>(body, objExp, keyParaExp, valueParaExp);
            return expression.Compile();
        }

        private Func<object, IEnumerator> _getKeys;
        public Func<object, IEnumerator> GetKeys => _getKeys = _getKeys ?? BuildGetKeysMethod(Type);

        protected virtual Func<object, IEnumerator> BuildGetKeysMethod(Type type)
        {
            var objExp = Expression.Parameter(typeof(object), "dic");
            var dicExp = Expression.TypeAs(objExp, Type);

            var property = type.GetProperty(nameof(IDictionary.Keys));
            var propertyExp = Expression.Property(dicExp, property);
            Expression body = null;
            if (typeof(IEnumerator).IsAssignableFrom(property.PropertyType))
            {
                body = Expression.TypeAs(propertyExp, typeof(IEnumerator));
            }
            else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                var method = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator));
                body = Expression.Call(propertyExp, method);
            }
            else
            {
                throw new JsonException($"未知的keys类型{property.PropertyType},类型：{Type}");
            }
            var expression = Expression.Lambda<Func<object, IEnumerator>>(body, objExp);
            return expression.Compile();
        }

        private Func<object, object, object> _getValue;
        public Func<object, object, object> GetValue => _getValue = _getValue ?? BuildGetValueMethod(Type);

        protected virtual Func<object, object, object> BuildGetValueMethod(Type type)
        {
            var objExp = Expression.Parameter(typeof(object), "dic");
            var keyParaExp = Expression.Parameter(typeof(object), "key");

            var dicExp = Expression.TypeAs(objExp, Type);
            Expression keyExp = KeyType != typeof(object)
                                ? (Expression)Expression.Convert(keyParaExp, KeyType)
                                : keyParaExp;
            var property = type.GetProperty("Item", new Type[] { KeyType });
            var indexExp = Expression.MakeIndex(dicExp, property, new Expression[] { keyExp });
            var body = Expression.TypeAs(indexExp, typeof(object));
            var expression = Expression.Lambda<Func<object, object, object>>(body, objExp, keyParaExp);
            return expression.Compile();
        }

        public override bool CanConvert(Type type)
        {
            throw new NotImplementedException();
        }

        public override TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            if (type.IsGenericType)
            {
                var arguments = type.GetGenericArguments();
                return new DictionaryConverter(type, arguments[0], arguments[1], provider);
            }
            return new DictionaryConverter(type, typeof(object), typeof(object), provider);
        }

        public override object FromReader(JsonReader read)
        {
            throw new NotImplementedException();
        }

        public override void WriteTo(JsonWriter write, object obj)
        {
            throw new NotImplementedException();
        }
    }
}
