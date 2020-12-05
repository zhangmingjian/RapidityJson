using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rapidity.Json.Converters
{
    internal abstract class TypeConverterBase : ITypeConverter
    {
        public Type Type { get; protected set; }

        public TypeConverterBase(Type type)
        {
            this.Type = type;
        }

        private Func<object> _createInstance;

        public virtual Func<object> CreateInstance
            => _createInstance = _createInstance ?? BuildCreateInstanceMethod(this.Type);

        /// <summary>
        /// 使用反射构造
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Func<object> BuildCreateInstanceMethod1(Type type)
        {
            Func<object> func = () =>
            {
                //优先获取无参构造函数
                //查找参数最少的一个构造函数
                var constructors = type.GetConstructors().OrderBy(t => t.GetParameters().Length);
                if (constructors.Count() == 0)
                    return Activator.CreateInstance(type);
                var parameters = constructors.First().GetParameters();
                List<object> parametValues = new List<object>();
                foreach (var para in parameters)
                {
                    //有参构造函数使用默认值填充
                    var defaultValue = GetDefaultValue(para.ParameterType);
                    parametValues.Add(defaultValue);
                }
                return Activator.CreateInstance(type, parametValues.ToArray());
            };
            return func;
        }

        /// <summary>
        /// 表达式树生成的委托来构造
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Func<object> BuildCreateInstanceMethod(Type type)
        {
            NewExpression newExp;
            //查找参数最少的一个构造函数
            var constructors = type.GetConstructors().OrderBy(t => t.GetParameters().Length);
            if (constructors.Count() == 0)
                newExp = Expression.New(type);
            else
            {
                var constructor = constructors.First();
                var parameters = constructor.GetParameters();
                List<Expression> parametExps = new List<Expression>();
                foreach (var para in parameters)
                {
                    //有参构造函数使用默认值填充
                    var defaultValue = GetDefaultValue(para.ParameterType);
                    ConstantExpression constant = Expression.Constant(defaultValue);
                    var paraValueExp = Expression.Convert(constant, para.ParameterType);
                    parametExps.Add(paraValueExp);
                }
                newExp = Expression.New(constructor, parametExps);
            }

            var body = Expression.TypeAs(newExp, typeof(object));
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(body);
            return expression.Compile();
        }

        protected virtual object GetDefaultValue(Type type)
        {
            //var defaultExp = Expression.Default(type);
            //var body = Expression.TypeAs(defaultExp, typeof(object));
            //var expression = Expression.Lambda<Func<object>>(body);
            //return expression.Compile()();
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public abstract object FromReader(JsonReader reader, JsonOption option);

        public abstract object FromElement(JsonElement element, JsonOption option);

        public virtual void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (obj == null) writer.WriteNull();
            else
            {
                var convert = option.ConverterProvider.Build(obj.GetType());
                convert.ToWriter(writer, obj, option);
            }
        }

        public virtual bool HandleLoopReferenceValue(JsonWriter writer, object value, JsonOption option)
        {
            if (option.LoopReferenceChecker.Exsits(value))
            {
                switch (option.LoopReferenceProcess)
                {
                    case LoopReferenceProcess.Ignore:
                        return true;
                    case LoopReferenceProcess.Null:
                        writer.WriteNull();
                        return true;
                    case LoopReferenceProcess.Error:
                        throw new JsonException($"对象：{value.GetType().Name}存在循环引用，序列化{value.GetType()}失败");
                }
            }
            return false;
        }

        /// <summary>
        /// 处理对象循环引用
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public virtual bool HandleLoopReferenceValue(JsonWriter writer, string propertyName, object value, JsonOption option)
        {
            if (option.LoopReferenceChecker.Exsits(value))
            {
                switch (option.LoopReferenceProcess)
                {
                    case LoopReferenceProcess.Ignore:
                        return true;
                    case LoopReferenceProcess.Null:
                        writer.WritePropertyName(propertyName);
                        writer.WriteNull();
                        return true;
                    case LoopReferenceProcess.Error:
                        throw new JsonException($"属性{propertyName}：{value.GetType().FullName}存在循环引用，序列化{value.GetType()}失败");
                }
            }
            return false;
        }
    }
}
