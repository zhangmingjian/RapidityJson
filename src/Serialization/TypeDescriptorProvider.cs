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
        public virtual TypeDescriptor Build(Type type)
        {
            if (_dictionary.ContainsKey(type)) return _dictionary[type];
            var descriptor = GetDescriptor(type);
            _dictionary.TryAdd(type, descriptor);
            return descriptor;
        }

        private TypeDescriptor GetDescriptor(Type type)
        {
            if (type.IsValueType || type == typeof(string))
                return new ValueDescriptor(type);
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                if(type.IsClass && !type.IsAbstract)
                    return new EnumerableDescriptor(type);
                var itemType = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(itemType);
                if (type.IsAssignableFrom(listType))
                    return new EnumerableDescriptor(listType);
            }
            if (type.IsArray)
                return new ArrayDescriptor(type);
            if (type.IsClass && !type.IsAbstract && !type.IsInterface)
                return new ObjectDescriptor(type);
            //if (typeof(IDictionary).IsAssignableFrom(type))
            //    return new DictionaryDescorptorProvider();
            throw new JsonException($"无法创建{type}的TypeDescriptor");
        }
    }
}