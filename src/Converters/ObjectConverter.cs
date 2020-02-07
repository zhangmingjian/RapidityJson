using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ObjectConverter : TypeConverter
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

        public override bool CanConvert(Type type)
        {
            throw new NotImplementedException();
        }

        public override TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return null;
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
