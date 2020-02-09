﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal abstract class EnumerableConverter : TypeConverter, IConverterCreator
    {
        public Type ItemType { get; protected set; }

        public EnumerableConverter(Type type, Type itemType, TypeConverterProvider provider) : base(type, provider)
        {
            ItemType = itemType;
        }

        private Action<object, object> _addItem;
        public Action<object, object> AddItem => _addItem = _addItem ?? BuildAddItemMethod(Type);

        protected virtual Action<object, object> BuildAddItemMethod(Type type)
        {
            var listExp = Expression.Parameter(typeof(object), "list");
            var itemExp = Expression.Parameter(typeof(object), "item");
            var instanceExp = Expression.Convert(listExp, type);
            var argumentExp = Expression.Convert(itemExp, ItemType);
            var addMethod = type.GetMethod(AddMethodName);
            var callExp = Expression.Call(instanceExp, addMethod, argumentExp);
            Expression<Action<object, object>> addItemExp = Expression.Lambda<Action<object, object>>(callExp, listExp, itemExp);
            return addItemExp.Compile();
        }

        protected virtual string AddMethodName => "Add";

        private Func<object, IEnumerator> _getEnumerator;

        public Func<object, IEnumerator> GetEnumerator => _getEnumerator = _getEnumerator ?? BuildGetEnumberatorMethod(Type);

        private Func<object, IEnumerator> BuildGetEnumberatorMethod(Type type)
        {
            var paramExp = Expression.Parameter(typeof(object), "list");
            var listExp = Expression.TypeAs(paramExp, type);
            var method = type.GetMethod(nameof(IEnumerable.GetEnumerator));
            var callExp = Expression.Call(listExp, method);
            var body = Expression.TypeAs(callExp,typeof(IEnumerator));
            var expression = Expression.Lambda<Func<object, IEnumerator>>(body, paramExp);
            return expression.Compile();
        }

        public abstract bool CanConvert(Type type);

        public abstract TypeConverter Create(Type type, TypeConverterProvider provider);

        public override object FromReader(JsonReader reader)
        {
            object instance = null;
            var convert = Provider.Build(ItemType);
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray: return instance;
                    case JsonTokenType.StartArray:
                        if (instance == null) instance = CreateInstance();
                        else
                        {
                            AddItem(instance, convert.FromReader(reader));
                        }
                        break;
                    case JsonTokenType.StartObject:
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        var valueItem = convert.FromReader(reader);
                        AddItem(instance, valueItem);
                        break;
                    case JsonTokenType.Null:
                        if (instance == null) return instance;
                        AddItem(instance, null);
                        break;
                    case JsonTokenType.None: break;
                }
            } while (reader.Read());
            if (instance == null) throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为：{JsonTokenType.StartArray}[", reader.Line, reader.Position);
            return instance;
        }

        public override object FromToken(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Null) return null;
            if (token.ValueType == JsonValueType.Array)
            {
                var list = CreateInstance();
                var arrayToken = (JsonArray)token;
                foreach (var item in arrayToken)
                {
                    var convert = Provider.Build(ItemType);
                    var itemValue = convert.FromToken(item);
                    AddItem(list, itemValue);
                }
                return list;
            }
            throw new JsonException($"无法从{token.ValueType}转换为{Type},反序列化{Type}失败");
        }

        public override void WriteTo(JsonWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            var enumer = GetEnumerator(obj);
            while (enumer.MoveNext())
            {
                var current = enumer.Current;
                if (current == null)
                {
                    writer.WriteNull();
                    continue;
                }
                var convert = Provider.Build(current.GetType());
                convert.WriteTo(writer, current);
            }
            writer.WriteEndArray();
        }
    }
}