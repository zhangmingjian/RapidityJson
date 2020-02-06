using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal class DictionaryDescriptor : TypeDescriptor
    {
        public override TypeKind TypeKind => TypeKind.Dictionary;

        public Type KeyType { get; protected set; }

        public Type ValueType { get; protected set; }

        public DictionaryDescriptor(Type type) : this(type, typeof(object), typeof(object))
        {
        }

        public DictionaryDescriptor(Type type, Type keyType, Type valueType) : base(type)
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
            var indexExp = Expression.MakeIndex(dicExp, property,new Expression[] { keyExp });
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
                throw new JsonException($"未知的keys类型{property.PropertyType},字典类型：{Type}");
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
    }

    /// <summary>
    /// 
    /// </summary>
    internal class StringKeyValueDescriptor : DictionaryDescriptor
    {
        public StringKeyValueDescriptor(Type type) : base(type, typeof(string), typeof(string))
        {
        }
    }
}
