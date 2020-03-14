using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    /// <summary>
    /// 
    /// </summary>
    internal class DictionaryConverter : TypeConverterBase, IConverterCreator
    {
        public Type KeyType { get; protected set; }

        public Type ValueType { get; protected set; }

        public DictionaryConverter(Type type, Type keyType, Type valueType) : base(type)
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
                //body = Expression.TypeAs(propertyExp, typeof(IEnumerator));
                body = propertyExp;
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
            Expression keyExp = Expression.Convert(keyParaExp, KeyType);
            var property = type.GetProperty("Item", new Type[] { KeyType });
            var indexExp = Expression.MakeIndex(dicExp, property, new Expression[] { keyExp });
            var body = Expression.TypeAs(indexExp, typeof(object));
            var expression = Expression.Lambda<Func<object, object, object>>(body, objExp, keyParaExp);
            return expression.Compile();
        }

        public virtual bool CanConvert(Type type)
        {
            if (type.IsGenericType)
            {
                var arguments = type.GetGenericArguments();
                if (arguments.Length == 2)
                {
                    var dicType = typeof(Dictionary<,>).MakeGenericType(arguments[0], arguments[1]);
                    if (dicType == type || type.IsAssignableFrom(dicType))
                        return true;
                    var sortDicType = typeof(SortedDictionary<,>).MakeGenericType(arguments[0], arguments[1]);
                    if (sortDicType == type || type.IsAssignableFrom(sortDicType))
                        return true;
                }
            }
            return false;
        }

        public virtual ITypeConverter Create(Type type)
        {
            var arguments = type.GetGenericArguments();
            if (type.IsClass && !type.IsAbstract)
                return new DictionaryConverter(type, arguments[0], arguments[1]);
            var dicType = typeof(Dictionary<,>).MakeGenericType(arguments[0], arguments[1]);
            return new DictionaryConverter(dicType, arguments[0], arguments[1]);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            object instance = null;
            object key = null;
            var convert = option.ConverterProvider.Build(ValueType);
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None: break;
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.StartObject:
                        if (instance == null) instance = CreateInstance();
                        else
                            SetKeyValue(instance, key, convert.FromReader(reader, option));
                        break;
                    case JsonTokenType.PropertyName:
                        key = reader.Text;
                        break;
                    case JsonTokenType.StartArray:
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        SetKeyValue(instance, key, convert.FromReader(reader, option));
                        break;
                    case JsonTokenType.Null:
                        if (instance == null) return instance;
                        SetKeyValue(instance, key, null);
                        break;
                }
            } while (reader.Read());
            throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为:{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (token.ValueType)
            {
                case JsonValueType.Null: return null;
                case JsonValueType.Object:
                    var dic = CreateInstance();
                    var objToken = (JsonObject)token;
                    foreach (var property in objToken.GetAllProperty())
                    {
                        var convert = option.ConverterProvider.Build(ValueType);
                        var value = convert.FromToken(property.Value, option);
                        SetKeyValue(dic, property.Name, value);
                    }
                    return dic;
                default:
                    throw new JsonException($"无法从{token.ValueType}转换为{Type},{this.GetType().Name}反序列化{Type}失败");
            }
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            writer.WriteStartObject();
            var keys = GetKeys(obj);
            while (keys.MoveNext())
            {
                var value = GetValue(obj, keys.Current);
                if (value == null && option.IgnoreNullValue)
                    continue;
                var name = option.CamelCaseNamed ? keys.Current.ToString().ToCamelCase() : keys.Current.ToString();
                //对象循环引用处理
                if (HandleLoopReferenceValue(writer, name, value, option))
                    continue;
                writer.WritePropertyName(name);
                base.ToWriter(writer, value, option);
            }
            writer.WriteEndObject();
            option.LoopReferenceChecker.PopObject();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class StringKeyValueConverter : DictionaryConverter, IConverterCreator
    {

        public StringKeyValueConverter(Type type) : base(type, typeof(string), typeof(string))
        {
        }

        public override bool CanConvert(Type type)
        {
            return type == typeof(StringDictionary) || type == typeof(NameValueCollection);
        }

        public override ITypeConverter Create(Type type)
        {
            return new StringKeyValueConverter(type);
        }
    }
}
