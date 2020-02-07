using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class EnumerableConverter : TypeConverter
    {
        public Type ItemType { get; protected set; }

        public EnumerableConverter(Type type, Type itemType, TypeConverterProvider provider) : base(type, provider)
        {
            ItemType = itemType;
        }

        private Action<object, object> _addItem;
        public Action<object, object> AddItem => _addItem = _addItem ?? BuildAddItemMethod(Type);

        protected virtual Action<object, object> BuildAddItemMethod(Type type)
        {
            var listExp = Expression.Parameter(typeof(object), "list");
            var itemExp = Expression.Parameter(typeof(object), "item");
            var instanceExp = Expression.Convert(listExp, type);
            var argumentExp = Expression.Convert(itemExp, ItemType);
            var addMethod = type.GetMethod(AddMethodName);
            var callExp = Expression.Call(instanceExp, addMethod, argumentExp);
            Expression<Action<object, object>> addItemExp = Expression.Lambda<Action<object, object>>(callExp, listExp, itemExp);
            return addItemExp.Compile();
        }

        protected string AddMethodName { get; set; } = "Add";

        private Func<object, IEnumerator> _getEnumerator;

        public Func<object, IEnumerator> GetEnumerator => _getEnumerator = _getEnumerator ?? BuildGetEnumberatorMethod(Type);

        private Func<object, IEnumerator> BuildGetEnumberatorMethod(Type type)
        {
            var paramExp = Expression.Parameter(typeof(object));
            var listExp = Expression.TypeAs(paramExp, type);
            var method = type.GetMethod(nameof(IEnumerable.GetEnumerator));
            var callExp = Expression.Call(listExp, method);
            var body = Expression.TypeAs(callExp, typeof(IEnumerator));
            var expression = Expression.Lambda<Func<object, IEnumerator>>(body, paramExp);
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

        public override object FromReader(JsonReader read)
        {
            throw new NotImplementedException();
        }

        public override void WriteTo(JsonWriter write, object obj)
        {
            throw new NotImplementedException();
        }
    }
}
