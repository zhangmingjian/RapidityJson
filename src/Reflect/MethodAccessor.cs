using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapidity.Json.Reflect
{
    internal class MethodAccessor : IMethodAccessor
    {
        public string Name => MethodInfo.Name;

        public MethodInfo MethodInfo { get; }

        public MethodAccessor(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            this.MethodInfo = methodInfo;
        }

        private Func<object, object[], object> _methodInvoke;
        private Func<object, object[], object> MethodInvokeFactory()
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var argsParameter = Expression.Parameter(typeof(object[]), "args");

            // build parameter list
            var paramExps = new List<Expression>();
            var paramInfos = MethodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // 取出实参并转换
                var valueObj = Expression.ArrayIndex(argsParameter, Expression.Constant(i));

                Type parameterType = paramInfos[i].ParameterType;
                if (parameterType.IsByRef) parameterType = parameterType.GetElementType();

                var valueCast = parameterType.IsValueType
                        ? Expression.Convert(valueObj, parameterType)
                        : Expression.TypeAs(valueObj, parameterType);

                paramExps.Add(valueCast);
            }

            // 静态方法调用
            var instanceCast = MethodInfo.IsStatic ? null : Expression.Convert(instanceParameter, MethodInfo.ReflectedType);

            // 实例方法调用
            var methodCall = Expression.Call(instanceCast, MethodInfo, paramExps);

            // 是否有返回值处理
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, argsParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, argsParameter);

                return lambda.Compile();
            }
        }

        public object Invoke(object instance, params object[] args)
        {
            if (_methodInvoke == null) _methodInvoke = MethodInvokeFactory();
            return _methodInvoke(instance, args);
        }

        internal static int GetKey(string name, IEnumerable<Type> parameterTypes)
        {
            unchecked
            {
                int result = name?.GetHashCode() ?? 0;
                result = parameterTypes.Aggregate(result, (r, p) => (r * 397) ^ (p != null ? p.GetHashCode() : 0));
                return result;
            }
        }
    }
}
