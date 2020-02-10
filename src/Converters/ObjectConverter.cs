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
            => MemberDefinitions.FirstOrDefault(x => x.PropertyName.Equals(name, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<MemberDefinition> GetMemberDefinitions(Type type)
        {
            var list = new List<MemberDefinition>();
            if (type == null) return list;
            foreach (var property in type.GetProperties())
            {
                //无读权限，静态属性跳过
                if (!property.CanRead || property.GetGetMethod().IsStatic) continue;
                //跳过委托
                if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) continue;
                var attr = property.GetCustomAttribute<PropertyAttribute>();
                if (attr != null && attr.Ignore) continue;
                list.Add(new MemberDefinition(property, attr));
            }
            foreach (var field in type.GetFields())
            {
                //跳过委托，静态字段
                if (field.IsStatic || typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
                var attr = field.GetCustomAttribute<PropertyAttribute>();
                if (attr != null && attr.Ignore) continue;
                list.Add(new MemberDefinition(field, attr));
            }
            return list.OrderBy(o => o.Sort);
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
                && !type.IsAbstract
                && type != typeof(object)
                && type != typeof(DBNull) //不是DBnull
                && !typeof(IEnumerable).IsAssignableFrom(type) //不是迭代器
                && !typeof(Delegate).IsAssignableFrom(type)    //不是委托
                )
                return true;
            //除datetime/decimal/guid以外的struct
            if (IsCustomStruct(type)) return true;
            return false;
        }

        public TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return new ObjectConverter(type, provider);
        }

        private bool IsCustomStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive //struct
                 && type != typeof(DateTime)
                 && type != typeof(decimal)
                 && type != typeof(Guid);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            object instance = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None: break;
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.StartObject:
                        if (instance == null) instance = CreateInstance();
                        break;
                    case JsonTokenType.PropertyName:
                        var member = GetMemberDefinition(reader.Text);
                        var converter = Provider.Build(member?.MemberType ?? typeof(object));
                        reader.Read();
                        var value = converter.FromReader(reader, option);
                        member?.SetValue(instance, value);
                        break;
                    case JsonTokenType.Null:
                        if (instance == null) return instance;
                        break;
                    default: throw new JsonException($"无效的JSON Token:{reader.TokenType}", reader.Line, reader.Position);
                }
            }
            while (reader.Read());
            if (instance == null) throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type}, 应为{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
            return instance;
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            if (token.ValueType == JsonValueType.Null) return null;
            if (token.ValueType == JsonValueType.Object)
            {
                var instance = CreateInstance();
                var objToken = (JsonObject)token;
                foreach (var property in objToken.GetAllProperty())
                {
                    var member = GetMemberDefinition(property.Name);
                    if (member != null)
                    {
                        var convert = Provider.Build(member.MemberType);
                        var value = convert.FromToken(property.Value, option);
                        member.SetValue(instance, value);
                    }
                }
                return instance;
            }
            throw new JsonException($"无法从{token.ValueType}转换为{Type},{this.GetType().Name}反序列化{Type}失败");
        }

        public override void WriteTo(JsonWriter writer, object obj, JsonOption option)
        {
            writer.WriteStartObject();
            foreach (var member in this.MemberDefinitions)
            {
                var value = GetValue(obj, member.PropertyName);
                if (value == null && option.IgnoreNullValue) continue;
                //属性循环引用：属性类型与当前类相同，且同一实例时
                if (value != null && member.MemberType == obj.GetType()
                    && obj.GetHashCode() == value.GetHashCode()) continue;
                var name = option.CamelCaseNamed ? member.PropertyName.ToCamelCase() : member.PropertyName;
                writer.WritePropertyName(name);
                base.WriteTo(writer, value, option);
            }
            writer.WriteEndObject();
        }
    }

    internal class MemberDefinition
    {
        public MemberDefinition(MemberInfo memberInfo, PropertyAttribute attribute)
        {
            this.MemberInfo = memberInfo;
            if (MemberInfo.MemberType == MemberTypes.Property) MemberType = ((PropertyInfo)MemberInfo).PropertyType;
            else if (MemberInfo.MemberType == MemberTypes.Field) MemberType = ((FieldInfo)MemberInfo).FieldType;
            JsonProperty = attribute;
            PropertyName = JsonProperty?.Name ?? MemberInfo.Name;
            Sort = JsonProperty?.Sort ?? 0;
        }

        public MemberInfo MemberInfo { get; }
        /// <summary>
        /// jsonProperty settings
        /// </summary>
        public PropertyAttribute JsonProperty { get; }

        public string PropertyName { get; }

        public Type MemberType { get; }

        public int Sort { get; }

        private Func<object, object> _getValue;

        public Func<object, object> GetValue => _getValue = _getValue ?? BuildGetValueMethod();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Func<object, object> BuildGetValueMethod()
        {
            var instanceExp = Expression.Parameter(typeof(object), "instance");
            MemberExpression memberExp;
            if (MemberInfo is PropertyInfo property && property.GetGetMethod().IsStatic)
            {
                memberExp = Expression.Property(null, MemberInfo.DeclaringType, MemberInfo.Name);
            }
            else if (MemberInfo is FieldInfo field && field.IsStatic)
            {
                memberExp = Expression.Field(null, MemberInfo.DeclaringType, MemberInfo.Name);
            }
            else
            {
                var instanceTypeExp = Expression.Convert(instanceExp, MemberInfo.DeclaringType);
                memberExp = Expression.PropertyOrField(instanceTypeExp, MemberInfo.Name);
            }
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

            var isProperty = MemberInfo is PropertyInfo;
            Expression body;
            if (isProperty && !((PropertyInfo)MemberInfo).CanWrite)
                body = Expression.Label(Expression.Label());
            else
            {
                MemberExpression memberExp;
                //静态属性赋值
                if (isProperty && ((PropertyInfo)MemberInfo).GetSetMethod().IsStatic)
                {
                    memberExp = Expression.Property(null, MemberInfo.DeclaringType, MemberInfo.Name);
                }
                //静态字段赋值
                else if ((MemberInfo is FieldInfo field) && field.IsStatic)
                {
                    memberExp = Expression.Field(null, MemberInfo.DeclaringType, MemberInfo.Name);
                }
                else   //实例字段
                {
                    var instanceTypeExp = Expression.TypeAs(instanceExp, MemberInfo.DeclaringType);
                    memberExp = Expression.PropertyOrField(instanceTypeExp, MemberInfo.Name);
                }
                body = Expression.Assign(memberExp, Expression.Convert(valueExp, MemberType));
            }
            Expression<Action<object, object>> exp = Expression.Lambda<Action<object, object>>(body, instanceExp, valueExp);
            return exp.Compile();
        }
    }
}
