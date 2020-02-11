using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        protected virtual Func<object> BuildCreateInstanceMethod(Type type)
        {
            NewExpression newExp;
            //优先获取无参构造函数
            var constructor = type.GetConstructor(Array.Empty<Type>());
            if (constructor != null)
                newExp = Expression.New(type);
            else
            {
                constructor = type.GetConstructors().OrderBy(t => t.GetParameters().Length).FirstOrDefault();
                var parameters = constructor.GetParameters();
                List<Expression> parametExps = new List<Expression>();
                foreach (var para in parameters)
                {
                    var defaultValue = GetDefaultValue(para.ParameterType);
                    ConstantExpression constant = Expression.Constant(defaultValue);
                    var paraValueExp = Expression.Convert(constant, para.ParameterType);
                    parametExps.Add(paraValueExp);
                }
                newExp = Expression.New(constructor, parametExps);
            }
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        protected virtual object GetDefaultValue(Type type)
        {
            var defaultExp = Expression.Default(type);
            var body = Expression.TypeAs(defaultExp, typeof(object));
            var expression = Expression.Lambda<Func<object>>(body);
            return expression.Compile()();
        }

        public abstract object FromReader(JsonReader reader, JsonOption option);

        public abstract object FromToken(JsonToken token, JsonOption option);

        public virtual void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (obj == null) writer.WriteNull();
            else
            {
                var convert = option.ConverterFactory.Build(obj.GetType());
                convert.ToWriter(writer, obj, option);
            }
        }
    }
}
