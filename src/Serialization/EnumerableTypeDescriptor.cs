using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Serialization
{
    /// <summary>
    /// 可迭代集合类型
    /// </summary>
    internal class EnumerableDescriptor : TypeDescriptor
    {
        public EnumerableDescriptor(Type type) : base(type)
        {
            ItemType = type.GenericTypeArguments[0];
        }

        public override TypeKind TypeKind => TypeKind.List;

        public Type ItemType { get; protected set; }

        protected override Func<object> BuildCreateInstanceMethod(Type type)
        {
            NewExpression newExp = Expression.New(type);
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        private Action<object, object> _addItemMethod;
        public Action<object, object> AddItemMethod => _addItemMethod = _addItemMethod ?? BuildAddItemMethod(Type);

        protected virtual Action<object, object> BuildAddItemMethod(Type type)
        {
            var listExp = Expression.Parameter(typeof(object), "list");
            var itemExp = Expression.Parameter(typeof(object), "item");
            var instanceExp = Expression.TypeAs(listExp, type);
            var argumentExp = Expression.TypeAs(itemExp, ItemType);
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
            var callEnumerExp = Expression.Call(listExp, method);
            Expression<Func<object, IEnumerator>> expression = Expression.Lambda<Func<object, IEnumerator>>(callEnumerExp, paramExp);
            return expression.Compile();
        }
    }

    internal class ArrayDescriptor : EnumerableDescriptor
    {
        public ArrayDescriptor(Type type) : base(type)
        {
        }

        public override TypeKind TypeKind => TypeKind.Array;

        public Func<object, object> ToArrayMethod { get; set; }
    }
}