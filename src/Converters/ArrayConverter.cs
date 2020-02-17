using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ArrayConverter : EnumerableConverter, IConverterCreator
    {
        private Type _arrayType;

        public ArrayConverter(Type type, Type listType, Type itemType) :
            base(listType, itemType)
        {
            _arrayType = type;
        }

        protected override Func<object, IEnumerator> BuildGetEnumberatorMethod(Type type)
        {
            return base.BuildGetEnumberatorMethod(_arrayType);
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
            return type.IsArray;
        }

        public override ITypeConverter Create(Type type)
        {
            var elementType = type.GetElementType();
            var listType = typeof(List<>).MakeGenericType(elementType);
            return new ArrayConverter(type, listType, elementType);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            var list = base.FromReader(reader, option);
            return ToArray(list);
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            var list = base.FromToken(token, option);
            return ToArray(list);
        }
    }

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
