using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ObjectConverter : TypeConverter, IConverterCreator
    {
        public ObjectConverter(Type type, TypeConverterProvider provider) : base(type, provider)
        {
        }

        private IEnumerable<MemberDefinition> _memberDefinitions;

        public IEnumerable<MemberDefinition> MemberDefinitions
            => _memberDefinitions = _memberDefinitions ?? GetMemberDefinitions(Type);


        public MemberDefinition GetMemberDefinition(string name)
            => MemberDefinitions.FirstOrDefault(x => x.JsonProperty.Equals(name, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<MemberDefinition> GetMemberDefinitions(Type type)
        {
            var list = new List<MemberDefinition>();
            foreach (var property in type.GetProperties())
            {
                if (!property.CanRead) continue;
                list.Add(new MemberDefinition(property));
            }
            foreach (var field in type.GetFields())
            {
                list.Add(new MemberDefinition(field));
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void SetValue(string memberName, Action<MemberDefinition> action)
        {
            var prop = GetMemberDefinition(memberName);
            if (prop != null)
            {
                action(prop);
            }
        }

        public object GetValue(object instace, string memberName)
        {
            var prop = GetMemberDefinition(memberName);
            if (prop != null)
                return prop.GetValue(instace);
            throw new JsonException($"类型{Type}不包含成员{memberName}");
        }

        public bool CanConvert(Type type)
        {
            if (type.IsClass
                && type != typeof(object)
                && type != typeof(DBNull)
                && !type.IsAbstract
                && !typeof(IEnumerable).IsAssignableFrom(type)
                && !typeof(Delegate).IsAssignableFrom(type))
                return true;
            //值类型且不是基元类型
            if (type.IsValueType
                && !type.IsPrimitive
                && type != typeof(DateTime)
                && type != typeof(decimal)
                && type != typeof(Guid))
                return true;
            return false;
        }

        public TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return new ObjectConverter(type, provider);
        }

        public override object FromReader(JsonReader reader)
        {
            object instance = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject: instance = CreateInstance(); break;
                    case JsonTokenType.Null:
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.PropertyName:
                        var property = GetMemberDefinition(reader.Value);
                        var converter = Provider.Build(property.MemberType);
                        reader.Read();
                        var value = converter.FromReader(reader);
                        property?.SetValue(instance, value);
                        break;
                    case JsonTokenType.None: break;
                    default: throw new JsonException($"无效的JSON Token:{reader.TokenType}", reader.Line, reader.Position);
                }
            }
            while (reader.Read());
            if (instance == null) throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type}, 应为{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
            return instance;
        }

        public override object FromToken(JsonToken token)
        {
            throw new NotImplementedException();
        }

        public override void WriteTo(JsonWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartObject();
            foreach (var member in this.MemberDefinitions)
            {
                writer.WritePropertyName(member.JsonProperty);
                var convert = Provider.Build(obj.GetType());
                var value = GetValue(obj, member.JsonProperty);
                convert.WriteTo(writer, value);
            }
            writer.WriteEndObject();
        }
    }

    internal class MemberDefinition
    {
        public MemberInfo MemberInfo { get; }

        public MemberDefinition(MemberInfo memberInfo)
        {
            this.MemberInfo = memberInfo;
            if (MemberInfo.MemberType == MemberTypes.Property) MemberType = ((PropertyInfo)MemberInfo).PropertyType;
            else if (MemberInfo.MemberType == MemberTypes.Field) MemberType = ((FieldInfo)MemberInfo).FieldType;
            JsonProperty = MemberInfo.Name;
        }

        public string JsonProperty { get; }

        public Type MemberType { get; }

        private Func<object, object> _getValue;

        public Func<object, object> GetValue => _getValue = _getValue ?? BuildGetValueMethod();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Func<object, object> BuildGetValueMethod()
        {
            var instanceExp = Expression.Parameter(typeof(object), "instance");
            var instanceTypeExp = Expression.TypeAs(instanceExp, MemberInfo.DeclaringType);
            MemberExpression memberExp = Expression.PropertyOrField(instanceTypeExp, MemberInfo.Name);
            var body = Expression.TypeAs(memberExp, typeof(object));
            Expression<Func<object, object>> exp = Expression.Lambda<Func<object, object>>(body, instanceExp);
            return exp.Compile();
        }

        private Action<object, object> _setValue;
        public Action<object, object> SetValue => _setValue = _setValue ?? BuildSetValueMethod();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Action<object, object> BuildSetValueMethod()
        {
            var instanceExp = Expression.Parameter(typeof(object), "instance");
            var valueExp = Expression.Parameter(typeof(object), "memberValue");

            Expression body;
            if (MemberInfo is PropertyInfo property && !property.CanWrite)
                body = Expression.Label(Expression.Label());
            else
            {
                var instanceTypeExp = Expression.TypeAs(instanceExp, MemberInfo.DeclaringType);
                MemberExpression memberExp = Expression.PropertyOrField(instanceTypeExp, MemberInfo.Name);
                body = Expression.Assign(memberExp, Expression.Convert(valueExp, MemberType));
            }
            Expression<Action<object, object>> exp = Expression.Lambda<Action<object, object>>(body, instanceExp, valueExp);
            return exp.Compile();
        }
    }
}
