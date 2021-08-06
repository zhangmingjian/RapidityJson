using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Reflect
{
    public class PropertyAccessor : IMemberAccessor
    {
        public PropertyInfo PropertyInfo { get; }

        public MemberInfo MemberInfo => PropertyInfo;

        public Type MemberType => PropertyInfo.PropertyType;

        public string Name => PropertyInfo.Name;

        public bool CanGet => PropertyInfo.CanRead;

        public bool CanSet => PropertyInfo.CanWrite;

        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            PropertyInfo = propertyInfo;
        }

        private Func<object, object> _getValue;

        private Func<object, object> GetValueFactory()
        {
            var instanceArgExp = Expression.Parameter(typeof(object), "instance");
            MemberExpression memberExp;
            if (PropertyInfo.GetMethod.IsStatic)
            {
                memberExp = Expression.Property(null, PropertyInfo.DeclaringType, Name);
            }
            else
            {
                var instanceExp = PropertyInfo.DeclaringType.IsValueType
                    ? Expression.Convert(instanceArgExp, PropertyInfo.DeclaringType)
                    : Expression.TypeAs(instanceArgExp, PropertyInfo.DeclaringType);
                memberExp = Expression.Property(instanceExp, Name);
            }
            var body = Expression.TypeAs(memberExp, typeof(object));
            return Expression.Lambda<Func<object, object>>(body, instanceArgExp).Compile();
        }

        public object GetValue(object instance)
        {
            if (!CanGet) throw new Exception($"属性{Name}不支持Get访问器");
            if (_getValue == null) _getValue = GetValueFactory();
            return _getValue(instance);
        }

        private Action<object, object> _setValue;
        private Action<object, object> SetValueFactory()
        {
            var instanceArgExp = Expression.Parameter(typeof(object), "instance");
            var valueArgExp = Expression.Parameter(typeof(object), "value");

            MemberExpression memberExp;
            //静态属性赋值
            if (PropertyInfo.SetMethod.IsStatic)
            {
                memberExp = Expression.Property(null, PropertyInfo.DeclaringType, Name);
            }
            else //实例属性赋值
            {
                var instanceExp = PropertyInfo.DeclaringType.IsValueType
                    ? Expression.Convert(instanceArgExp, PropertyInfo.DeclaringType)
                    : Expression.TypeAs(instanceArgExp, PropertyInfo.DeclaringType);
                memberExp = Expression.Property(instanceExp, Name);
            }
            var valueExp = MemberType.IsValueType
                            ? Expression.Convert(valueArgExp, MemberType)
                            : Expression.TypeAs(valueArgExp, MemberType);
            var body = Expression.Assign(memberExp, valueExp);
            return Expression.Lambda<Action<object, object>>(body, instanceArgExp, valueArgExp).Compile();
        }

        public void SetValue(object instance, object value)
        {
            if (!CanSet) throw new Exception($"属性{Name}不支持Set访问器");
            if (_setValue == null) _setValue = SetValueFactory();
            _setValue(instance, value);
        }
    }
}
