using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    /// <summary>
    /// 泛型List<>类型Converter
    /// </summary>
    internal class ListConverter : EnumerableConverter, IConverterCreator
    {

        public ListConverter(Type type, Type itemType, TypeConverterProvider provider) :
            base(type, itemType, provider)
        {
        }

        public override bool CanConvert(Type type)
        {
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var args = type.GetGenericArguments();
                if (args.Length != 1) return false;
                var listType = typeof(List<>).MakeGenericType(args[0]);
                if (type.IsAssignableFrom(listType)) return true;
            }
            return false;
        }

        /// <summary>
        /// 创建泛型List<>类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public override TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            var itemType = type.GetGenericArguments()[0];
            if (type.IsClass && !type.IsAbstract)
                return new ListConverter(type, itemType, provider);
            return new ListConverter(typeof(List<>).MakeGenericType(itemType), itemType, provider);
        }
    }
}
