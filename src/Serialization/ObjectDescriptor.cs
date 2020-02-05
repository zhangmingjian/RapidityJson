using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    internal class ObjectDescriptor : TypeDescriptor
    {
        public ObjectDescriptor(Type type) : base(type)
        {
        }

        public override TypeKind TypeKind => TypeKind.Object;

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
        /// <param name="memberName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TrySetValue(object instance, string memberName, object value)
        {
            var prop = GetMemberDefinition(memberName);
            if (prop != null)
            {
                prop.SetValueMethod(instance, value);
                return true;
            }
            return false;
        }

        public object GetValue(object instace, string memberName)
        {
            var prop = GetMemberDefinition(memberName);
            if (prop != null)
                return prop.GetValueMethod(instace);
            throw new JsonException($"类型{Type}不包含成员{memberName}");
        }
    }

    internal class MemberDefinition
    {
        public MemberInfo MemberInfo { get; }

        public MemberDefinition(MemberInfo memberInfo)
        {
            this.MemberInfo = memberInfo;
            if (MemberInfo.MemberType == MemberTypes.Property) _memberType = ((PropertyInfo)MemberInfo).PropertyType;
            else if (MemberInfo.MemberType == MemberTypes.Field) _memberType = ((FieldInfo)MemberInfo).FieldType;
            JsonProperty = MemberInfo.Name;
        }

        public string JsonProperty { get; }

        private Type _memberType;

        public Type MemberType => _memberType;

        private Func<object, object> _getValueMethod;

        public Func<object, object> GetValueMethod => _getValueMethod = _getValueMethod ?? BuildGetValueMethod();

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

        private Action<object, object> _setValueMethod;
        public Action<object, object> SetValueMethod => _setValueMethod = _setValueMethod ?? BuildSetValueMethod();

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
