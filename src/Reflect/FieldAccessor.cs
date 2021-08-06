using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Reflect
{
    public class FieldAccessor : IMemberAccessor
    {
        public FieldInfo FieldInfo { get; }

        public MemberInfo MemberInfo => FieldInfo;

        public Type MemberType => FieldInfo.FieldType;

        public string Name => FieldInfo.Name;

        public bool CanGet => FieldInfo.IsPublic;

        public bool CanSet => FieldInfo.IsPublic;

        public FieldAccessor(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
            FieldInfo = fieldInfo;
        }

        private Func<object, object> _getValue;
        protected virtual Func<object, object> GetValueFactory()
        {
            var instanceArgExp = Expression.Parameter(typeof(object), "instance");
            MemberExpression memberExp;
            if (FieldInfo.IsStatic)
            {
                memberExp = Expression.Field(null, FieldInfo.DeclaringType, Name);
            }
            else
            {
                var instanceExp = FieldInfo.DeclaringType.IsValueType
                    ? Expression.Convert(instanceArgExp, FieldInfo.DeclaringType)
                    : Expression.TypeAs(instanceArgExp, FieldInfo.DeclaringType);
                memberExp = Expression.Field(instanceExp, Name);
            }
            var body = Expression.TypeAs(memberExp, typeof(object));
            return Expression.Lambda<Func<object, object>>(body, instanceArgExp).Compile();
        }

        public object GetValue(object instance)
        {
            if (_getValue == null) _getValue = GetValueFactory();
            return _getValue(instance);
        }

        private Action<object, object> _setValue;
        protected virtual Action<object, object> SetValueFactory()
        {
            var instanceArgExp = Expression.Parameter(typeof(object), "instance");
            var valueArgExp = Expression.Parameter(typeof(object), "value");

            MemberExpression memberExp;
            //静态字段赋值
            if (FieldInfo.IsStatic)
            {
                memberExp = Expression.Field(null, FieldInfo.DeclaringType, Name);
            }
            else //实例字段赋值
            {
                var instanceExp = FieldInfo.DeclaringType.IsValueType
                    ? Expression.Convert(instanceArgExp, FieldInfo.DeclaringType)
                    : Expression.TypeAs(instanceArgExp, FieldInfo.DeclaringType);
                memberExp = Expression.Field(instanceExp, Name);
            }
            var valueExp = MemberType.IsValueType
                            ? Expression.Convert(valueArgExp, MemberType)
                            : Expression.TypeAs(valueArgExp, MemberType);
            var body = Expression.Assign(memberExp, valueExp);
            return Expression.Lambda<Action<object, object>>(body, instanceArgExp, valueArgExp).Compile();
        }

        public void SetValue(object instance, object value)
        {
            if (_setValue == null) _setValue = SetValueFactory();
            _setValue(instance, value);
        }
    }
}
