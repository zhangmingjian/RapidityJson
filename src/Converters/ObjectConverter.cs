using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapidity.Json.Converters
{
    internal class ObjectConverter : TypeConverterBase, IConverterCreator
    {
        public ObjectConverter(Type type) : base(type)
        {
        }

        private Dictionary<string, MemberDefinition> _memberDefinitions;
        private Dictionary<string, MemberDefinition> MemberDefinitions => _memberDefinitions = _memberDefinitions ?? GetMemberDefinitions(Type);

        private IEnumerable<MemberDefinition> _memberList;

        public IEnumerable<MemberDefinition> MemberList => _memberList = _memberList ?? MemberDefinitions.Values.OrderBy(x => x.Sort);

        public MemberDefinition GetMemberDefinition(string name) => MemberDefinitions.TryGetValue(name, out MemberDefinition member) ? member : null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Dictionary<string, MemberDefinition> GetMemberDefinitions(Type type)
        {
            var dic = new Dictionary<string, MemberDefinition>(StringComparer.CurrentCultureIgnoreCase);
            if (type == null) return dic;
            foreach (var property in type.GetProperties())
            {
                //无读权限，静态属性跳过
                if (!property.CanRead || property.GetGetMethod().IsStatic
                    || typeof(Delegate).IsAssignableFrom(property.PropertyType))
                    continue;
                //跳过委托
                if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) continue;
                var attr = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (attr != null && attr.Ignore) continue;
                var member = new MemberDefinition(property, attr);
                //var member = new ReflectionMemberDefinition(property, attr);
                dic[member.PropertyName] = member;
            }
            foreach (var field in type.GetFields())
            {
                //跳过委托，静态字段
                if (field.IsStatic || typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
                var attr = field.GetCustomAttribute<JsonPropertyAttribute>();
                if (attr != null && attr.Ignore) continue;
                var member = new MemberDefinition(field, attr);
                //var member = new ReflectionMemberDefinition(field, attr);
                dic[member.PropertyName] = member;
            }
            return dic;
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

        public virtual bool CanConvert(Type type)
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

        public virtual ITypeConverter Create(Type type)
        {
            return new ObjectConverter(type);
        }

        private bool IsCustomStruct(Type type)
        {
            //除datetime/decimal/guid以外的struct
            return type.IsValueType && !type.IsPrimitive //struct
                 && type != typeof(decimal)
                 && type != typeof(DateTime)
                 && type != typeof(DateTimeOffset)
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
                        var converter = option.ConverterProvider.Build(member?.MemberType ?? typeof(object));
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
            throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type}, 应为{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (token.ValueType)
            {
                case JsonValueType.Null: return null;
                case JsonValueType.Object:
                    var instance = CreateInstance();
                    var objToken = (JsonObject)token;
                    foreach (var property in objToken.GetAllProperty())
                    {
                        var member = GetMemberDefinition(property.Name);
                        if (member != null)
                        {
                            var convert = option.ConverterProvider.Build(member.MemberType);
                            var value = convert.FromToken(property.Value, option);
                            member.SetValue(instance, value);
                        }
                    }
                    return instance;
                default:
                    throw new JsonException($"无法从{token.ValueType}转换为{Type},{this.GetType().Name}反序列化{Type}失败");
            }
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            writer.WriteStartObject();
            foreach (var member in this.MemberList)
            {
                var value = GetValue(obj, member.PropertyName);
                if (value == null && option.IgnoreNullValue)
                    continue;
                if (HandleLoopReferenceValue(writer, member.PropertyName, value, option))
                    continue;
                var name = option.CamelCaseNamed ? member.PropertyName.ToCamelCase() : member.PropertyName;
                writer.WritePropertyName(name);
                base.ToWriter(writer, value, option);
            }
            writer.WriteEndObject();
            option.LoopReferenceChecker.PopObject();
        }
    }

    internal class MemberDefinition
    {
        public MemberDefinition(MemberInfo memberInfo, JsonPropertyAttribute attribute)
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
        public JsonPropertyAttribute JsonProperty { get; }

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
                body = Expression.Label(Expression.Label()); //跳过只读属性赋值
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
                    var instanceTypeExp = Expression.Convert(instanceExp, MemberInfo.DeclaringType);
                    memberExp = Expression.PropertyOrField(instanceTypeExp, MemberInfo.Name);
                }
                body = Expression.Assign(memberExp, Expression.Convert(valueExp, MemberType));
            }
            Expression<Action<object, object>> exp = Expression.Lambda<Action<object, object>>(body, instanceExp, valueExp);
            return exp.Compile();
        }
    }

    internal class ReflectionMemberDefinition : MemberDefinition
    {
        public ReflectionMemberDefinition(MemberInfo memberInfo, JsonPropertyAttribute attribute) : base(memberInfo, attribute)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Func<object, object> BuildGetValueMethod()
        {
            Func<object, object> func = null;
            if (MemberInfo is PropertyInfo property)
            {
                func = instance => property.GetValue(property.GetGetMethod().IsStatic ? null : instance);
            }
            else
            {
                var field = (FieldInfo)MemberInfo;
                func = instance => field.GetValue(field.IsStatic ? null : instance);
            }
            return func;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Action<object, object> BuildSetValueMethod()
        {
            Action<object, object> action = null;
            if (MemberInfo is PropertyInfo property)
            {
                if (!property.CanWrite) action = (instance, value) => { };
                else
                {
                    var isStatic = property.GetSetMethod().IsStatic;
                    action = (instance, value) => property.SetValue(isStatic ? null : instance, value);
                }
            }
            else
            {
                var field = (FieldInfo)MemberInfo;
                action = (instance, value) => field.SetValue(field.IsStatic ? null : instance, value);
            }
            return action;
        }
    }
}
