using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal class TypeDescriptorProvider
    {
        private static ConcurrentDictionary<Type, TypeDescriptor> _dictionary = new ConcurrentDictionary<Type, TypeDescriptor>();
        public virtual TypeDescriptor GetDescriptor(Type type)
        {
            if (_dictionary.ContainsKey(type)) return _dictionary[type];
            var provider = GetProvider(type);
            var descriptor = provider.GetDescriptor(type);
            _dictionary.TryAdd(type, descriptor);
            return descriptor;
        }

        protected virtual TypeDescriptorProvider GetProvider(Type type)
        {
            if (type.IsValueType || type == typeof(string))
                return new ValueDescriptorProvider();
            if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
                return new CollectionDescriptorProvider();
            if (type.IsArray)
                return new ListClassDescriptorProvider();
            if (typeof(IDictionary).IsAssignableFrom(type))
                return new DictionaryDescorptorProvider();
            if (type.IsClass && !type.IsAbstract && !type.IsInterface)
                return new ObjectDescriptorProvider();
            throw new JsonException($"无法创建{type}的TypeDescriptorProvider");
        }
    }

    internal class ValueDescriptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            object value;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: value = default(bool); break;
                case TypeCode.Byte: value = default(byte); break;
                case TypeCode.Char: value = default(char); break;
                case TypeCode.Int16: value = default(Int16); break;
                case TypeCode.UInt16: value = default(UInt16); break;
                case TypeCode.Int32: value = default(Int32); break;
                case TypeCode.UInt32: value = default(UInt32); break;
                case TypeCode.Int64: value = default(Int64); break;
                case TypeCode.UInt64: value = default(UInt64); break;
                case TypeCode.Single: value = default(Single); break;
                case TypeCode.Double: value = default(double); break;
                case TypeCode.Decimal: value = default(decimal); break;
                case TypeCode.SByte: value = default(sbyte); break;
                case TypeCode.DateTime: value = default(DateTime); break;
                case TypeCode.String: value = default(string); break;
                case TypeCode.DBNull: value = default(DBNull); break;
                default:
                    if (type.IsValueType)
                        value = Activator.CreateInstance(type);
                    else
                        value = default; break;
            }

            return new ValueDescriptor
            {
                TypeKind = TypeKind.Value,
                CreateMethod = new Func<object>(() => value)
            };
        }
    }

    internal class ObjectDescriptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            var descriptor = new ObjectDescriptor();
            descriptor.CreateMethod = BuildCreateMethod(type);
            descriptor.PropertyDescriptors = GetPropertyDescriptors(type);
            return descriptor;
        }

        private Func<object> BuildCreateMethod(Type type)
        {
            var constructor = type.GetConstructors().OrderBy(t => t.GetParameters().Length).FirstOrDefault();
            var parameters = constructor.GetParameters();
            NewExpression newExp;
            if (parameters.Length == 0)
                newExp = Expression.New(type);
            else
            {
                List<Expression> parametExps = new List<Expression>();
                foreach (var para in parameters)
                {
                    var desc = base.GetDescriptor(para.ParameterType);
                    ConstantExpression constant = Expression.Constant(desc.CreateMethod(), para.ParameterType);
                    parametExps.Add(constant);
                }
                newExp = Expression.New(constructor, parametExps);
            }
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        private List<PropertyDescriptor> GetPropertyDescriptors(Type type)
        {
            var list = new List<PropertyDescriptor>();
            var instanceExp = Expression.Parameter(typeof(object), "instance");
            var instanceTypeExp = Expression.TypeAs(instanceExp, type);
            foreach (var property in type.GetProperties())
            {
                if (!property.CanWrite) continue;
                var descr = new PropertyDescriptor
                {
                    MemberInfo = property,
                    JsonAlias = property.Name,
                    MemberType = property.PropertyType
                };
                var valueExp = Expression.Parameter(typeof(object), "propertyValue");
                var propertyExp = Expression.Property(instanceTypeExp, property);
                var body = Expression.Assign(propertyExp, Expression.Convert(valueExp, property.PropertyType));
                Expression<Action<object, object>> exp = Expression.Lambda<Action<object, object>>(body, instanceExp, valueExp);
                descr.SetValueMethod = exp.Compile();

                list.Add(descr);
            }
            return list;
        }

        private Type GetNullableType(Type type)
        {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
            if (type.IsValueType)
                return typeof(Nullable<>).MakeGenericType(type);
            else
                return type;
        }

        private Action<object, string, object> BuildSetMemberMethod(Type type)
        {
            var instanceExp = Expression.Parameter(typeof(object), "_obj");
            var nameExp = Expression.Parameter(typeof(string), "_name");
            var valueExp = Expression.Parameter(typeof(object), "_value");
            var instanceTypeExp = Expression.TypeAs(instanceExp, type);

            Expression body = null;
            foreach (var property in type.GetProperties())
            {
                if (!property.CanWrite) continue;
                var propertyExp = Expression.Property(instanceTypeExp, property);
                //获取property.name
                var propExp = Expression.Constant(property);
                var propNameValueExp = Expression.Property(propExp, "Name");
                var equalsMethod = typeof(string).GetMethod(nameof(string.Equals), new Type[] { typeof(string), typeof(StringComparison) });
                //比较propertyName是否与传入的属性相同
                var equal = Expression.Call(propNameValueExp, equalsMethod, nameExp, Expression.Constant(StringComparison.CurrentCultureIgnoreCase));
                //当匹配到属性时 赋值
                body = Expression.IfThen(equal, Expression.Assign(propertyExp, Expression.Convert(valueExp, property.PropertyType)));
            }
            foreach (var filed in type.GetFields())
            {
                if (!filed.IsPublic) continue;
                //memberExps.Add(Expression.Field(instanceExp, filed));
            }
            Expression<Action<object, string, object>> exp = Expression.Lambda<Action<object, string, object>>(body, instanceExp, nameExp, valueExp);
            return exp.Compile();
        }
    }

    internal class CollectionDescriptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            var provider = GetProvider(type);
            return provider.GetDescriptor(type);
        }

        protected override TypeDescriptorProvider GetProvider(Type type)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                return new ListClassDescriptorProvider();
            }
            return base.GetProvider(type);
        }
    }

    internal class ListClassDescriptorProvider : CollectionDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            var descriptor = new ListDescriptor { TypeKind = TypeKind.List };
            var itemType = type.GetGenericArguments()[0];
            var listType = typeof(List<>);
            listType = listType.MakeGenericType(itemType);
            descriptor.CreateMethod = BuildCreateMethod(listType);
            descriptor.AddMethod = BuildAddMethod(listType, itemType);
            return descriptor;
        }

        protected virtual Func<object> BuildCreateMethod(Type type)
        {
            NewExpression newExp = Expression.New(type);
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        protected virtual MethodInfo GetAddMethod(Type type) => type.GetMethod("Add");

        protected virtual Action<object, object> BuildAddMethod(Type type, Type itemType)
        {
            var listExp = Expression.Parameter(typeof(object), "_list");
            var itemExp = Expression.Parameter(typeof(object), "_item");
            var instanceExp = Expression.TypeAs(listExp, type);
            var argumentExp = Expression.TypeAs(itemExp, itemType);
            var callExp = Expression.Call(instanceExp, GetAddMethod(type), argumentExp);
            Expression<Action<object, object>> addItemExp = Expression.Lambda<Action<object, object>>(callExp, listExp, itemExp);
            return addItemExp.Compile();
        }
    }

    internal class DictionaryDescorptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            throw new NotImplementedException();
        }
    }
}