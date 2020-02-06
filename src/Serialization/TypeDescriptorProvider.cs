using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            switch (Type.GetTypeCode(type))
            {
                //判断范围应由小到大，由具体到抽象，以免误判
                case TypeCode.Object:
                    if (type == typeof(Guid) || Nullable.GetUnderlyingType(type) != null)
                        return new ValueDescriptor(type);
                    if (type == typeof(StringDictionary) || type == typeof(NameValueCollection))
                        return new StringKeyValueDescriptor(type);
                    if (typeof(IDictionary).IsAssignableFrom(type))
                    {
                        //不是泛型 直接使用object key/value键值对字典
                        if (!type.IsGenericType) return new DictionaryDescriptor(type);
                        else
                        {
                            var arguments = type.GetGenericArguments();
                            if (arguments.Length == 2)
                            {
                                var keyType = arguments[0];
                                var valueType = arguments[1];
                                if (type.IsClass && !type.IsAbstract)
                                    return new DictionaryDescriptor(type, keyType, valueType);
                                if (type.IsInterface || type.IsAbstract)
                                {
                                    var dicType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                                    if (type.IsAssignableFrom(dicType))
                                        return new DictionaryDescriptor(dicType, keyType, valueType);
                                }
                            }
                        }
                    }
                    if (type.IsArray)
                        return new ArrayDescriptor(type);
                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        if (!type.IsGenericType) return new EnumerableDescriptor(type);
                        else
                        {
                            var itemType = type.GetGenericArguments()[0];
                            if (type.IsClass && !type.IsAbstract)
                                return new EnumerableDescriptor(type, itemType);
                            var listType = typeof(List<>).MakeGenericType(itemType);
                            if (type.IsAssignableFrom(listType))
                                return new EnumerableDescriptor(listType, itemType);
                        }
                    }
                    if (type.IsClass && !type.IsAbstract && !type.IsInterface)
                        return new ObjectDescriptor(type);
                    throw new JsonException($"不支持的类型{type},无法创建{type}的TypeDescriptor");
                default: return new ValueDescriptor(type);
            }
        }
    }
}