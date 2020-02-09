using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rapidity.Json.Converters
{
    public abstract class TypeConverter
    {
        public TypeConverterProvider Provider { get; protected set; }

        public Type Type { get; protected set; }

        public TypeConverter(Type type, TypeConverterProvider provider)
        {
            this.Type = type;
            this.Provider = provider;
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
                    var converter = Provider.Build(para.ParameterType);
                    ConstantExpression constant = Expression.Constant(converter.CreateInstance());
                    parametExps.Add(constant);
                }
                newExp = Expression.New(constructor, parametExps);
            }
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        public abstract object FromReader(JsonReader reader, JsonOption option);

        public abstract object FromToken(JsonToken token, JsonOption option);

        public abstract void WriteTo(JsonWriter writer, object obj, JsonOption option);
    }
}
