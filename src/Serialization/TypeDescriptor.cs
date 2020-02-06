using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal enum TypeKind : byte
    {
        Value,
        Object,
        List,
        Dictionary
    }

    internal abstract class TypeDescriptor
    {
        public abstract TypeKind TypeKind { get; }

        public Type Type { get; protected set; }

        protected Func<object> _createInstance;

        public virtual Func<object> CreateInstance
        {
            get => _createInstance = _createInstance ?? BuildCreateInstanceMethod(this.Type);
            protected set => _createInstance = value;
        }

        protected TypeDescriptor(Type type)
        {
            this.Type = type;
        }

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
                    var desc = Create(para.ParameterType);
                    ConstantExpression constant = Expression.Constant(desc.CreateInstance(), para.ParameterType);
                    parametExps.Add(constant);
                }
                newExp = Expression.New(constructor, parametExps);
            }
            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(newExp);
            return expression.Compile();
        }

        public static TypeDescriptor Create(Type type)
        {
            return new TypeDescriptorProvider().Build(type);
        }
    }

    internal class ValueDescriptor : TypeDescriptor
    {
        public ValueDescriptor(Type type) : base(type)
        {
        }

        public override TypeKind TypeKind => TypeKind.Value;

        protected override Func<object> BuildCreateInstanceMethod(Type type)
        {
            object value;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: value = default(bool); break;
                case TypeCode.Byte: value = default(byte); break;
                case TypeCode.Char: value = default(char); break;
                case TypeCode.Int16: value = default(Int16); break;
                case TypeCode.UInt16: value = default(UInt16); break;
                case TypeCode.Int32: value = default(Int32); break;
                case TypeCode.UInt32: value = default(UInt32); break;
                case TypeCode.Int64: value = default(Int64); break;
                case TypeCode.UInt64: value = default(UInt64); break;
                case TypeCode.Single: value = default(Single); break;
                case TypeCode.Double: value = default(double); break;
                case TypeCode.Decimal: value = default(decimal); break;
                case TypeCode.SByte: value = default(sbyte); break;
                case TypeCode.DateTime: value = default(DateTime); break;
                case TypeCode.String: value = default(string); break;
                case TypeCode.DBNull: value = default(DBNull); break;
                default:
                    if (type.IsValueType)
                        value = Activator.CreateInstance(type);
                    else
                        value = default; break;
            }
            return () => value;
        }
    }
}
