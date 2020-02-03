using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal class TypeDescriptorProvider
    {
        protected static ConcurrentDictionary<Type, TypeDescriptor> Dictionary = new ConcurrentDictionary<Type, TypeDescriptor>();
        public virtual TypeDescriptor GetDescriptor(Type type)
        {
            if (Dictionary.ContainsKey(type)) return Dictionary[type];
            var provider = GetProvider(type);
            var descriptor = provider.GetDescriptor(type);
            Dictionary.TryAdd(type, descriptor);
            return descriptor;
        }

        public virtual TypeDescriptorProvider GetProvider(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Object:
                    if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType || type.IsArray)
                        return new CollectionDescriptorProvider();
                    if (typeof(IDictionary).IsAssignableFrom(type))
                        return new DictionaryDescorptorProvider();
                    if (type.IsClass && !type.IsAbstract && !type.IsInterface)
                        return new ObjectDescriptorProvider();
                    throw new JsonException($"无法创建{type}的TypeDescriptorProvider");
                default: return new ValueDescriptorProvider(typeCode);
            }
        }

        class ValueDescriptorProvider : TypeDescriptorProvider
        {
            private TypeCode _typeCode;
            public ValueDescriptorProvider(TypeCode typeCode)
            {
                _typeCode = typeCode;
            }

            public override TypeDescriptor GetDescriptor(Type type)
            {
                object value;
                switch (_typeCode)
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
                    default: value = null; break;
                }
                return new TypeDescriptor
                {
                    TypeKind = TypeKind.Value,
                    Create = new Func<object>(() => value)
                };
            }
        }
    }

    internal class ObjectDescriptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            var descriptor = new TypeDescriptor { TypeKind = TypeKind.Object };
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
                    ConstantExpression constant = Expression.Constant(desc.Create(), para.ParameterType);
                    parametExps.Add(constant);
                }
                newExp = Expression.New(constructor, parametExps);
            }
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            descriptor.Create = expression.Compile();
            return descriptor;
        }
    }

    internal class CollectionDescriptorProvider : TypeDescriptorProvider
    {
        public override TypeDescriptor GetDescriptor(Type type)
        {
            var provider = GetProvider(type);
            return provider.GetDescriptor(type);
        }

        public override TypeDescriptorProvider GetProvider(Type type)
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
            var descriptor = new ListDesciptor { TypeKind = type.IsArray ? TypeKind.Array : TypeKind.List };
            var genericType = type.GetGenericArguments()[0];
            var listType = typeof(List<>);
            listType = listType.MakeGenericType(genericType);
            var newExp = Expression.New(listType);
            Expression<Func<object>> createExp = Expression.Lambda<Func<object>>(newExp);
            descriptor.Create = createExp.Compile();

            var addMethod = listType.GetMethod("Add");
            var instanceExp = Expression.Parameter(type, "_list");
            var argumentExp = Expression.Parameter(genericType, "_item");
            var callExp = Expression.Call(instanceExp, addMethod, argumentExp);
            Expression<Action<object, object>> addItemExp = Expression.Lambda<Action<object, object>>(callExp, instanceExp, argumentExp);
            descriptor.Add = addItemExp.Compile();
            return descriptor;
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