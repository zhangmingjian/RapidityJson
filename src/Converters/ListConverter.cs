using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    /// <summary>
    /// 泛型List<>类型Converter
    /// </summary>
    internal class ListConverter : EnumerableConverter, IConverterCreator
    {

        public ListConverter(Type type, Type itemType) :  base(type, itemType)
        {
        }

        public override bool CanConvert(Type type)
        {
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var args = type.GetGenericArguments();
                if (args.Length != 1) return false;
                var listType = typeof(List<>).MakeGenericType(args[0]);
                var collectionType = typeof(Collection<>).MakeGenericType(args[0]);
                if (type == listType
                    || type == collectionType
                    || type.IsAssignableFrom(listType)
                    || type.IsAssignableFrom(collectionType))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 创建泛型List<>类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public override ITypeConverter Create(Type type)
        {
            var itemType = type.GetGenericArguments()[0];
            if (type.IsClass && !type.IsAbstract)
                return new ListConverter(type, itemType);
            var listType = typeof(List<>).MakeGenericType(itemType);
            if (type.IsAssignableFrom(listType))
                return new ListConverter(listType, itemType);
            var collectionType = typeof(Collection<>).MakeGenericType(itemType);
            if (type.IsAssignableFrom(collectionType))
                return new ListConverter(collectionType, itemType);
            return null;
        }
    }

    /// <summary>
    /// ArrayList类型Converter
    /// </summary>
    internal class ArrayListConverter : EnumerableConverter, IConverterCreator
    {
        public ArrayListConverter(Type type) : base(type, typeof(object))
        {
        }

        public override bool CanConvert(Type type)
        {
            return type == typeof(ArrayList);
        }

        public override ITypeConverter Create(Type type)
        {
            return new ArrayListConverter(type);
        }
    }
}
