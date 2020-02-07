using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ArrayConverter : EnumerableConverter
    {
        public ArrayConverter(Type type, TypeConverterProvider provider) :
            base(typeof(List<>).MakeGenericType(type.GetElementType()),
                type.GetElementType(),
                provider)
        {
        }

        private Func<object, object> _toArray;
        public Func<object, object> ToArray => _toArray = _toArray ?? BuildToArrayMethod(Type);

        protected Func<object, object> BuildToArrayMethod(Type type)
        {
            var objExp = Expression.Parameter(typeof(object), "list");

            var equalExp = Expression.Equal(objExp, Expression.Constant(null));

            var listExp = Expression.TypeAs(objExp, type);
            var method = type.GetMethod("ToArray");
            var call = Expression.Call(listExp, method);
            //判断入参是否为null
            var body = Expression.Condition(equalExp,
                                objExp,
                                Expression.TypeAs(call, typeof(object)));
            var expression = Expression.Lambda<Func<object, object>>(body, objExp);
            return expression.Compile();
        }

        public override bool CanConvert(Type type)
        {
            throw new NotImplementedException();
        }

        public override TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return null;
        }
    }
}
