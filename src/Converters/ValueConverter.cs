using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ValueConverter : TypeConverter
    {
        public ValueConverter(Type type, TypeConverterProvider provider) : base(type, provider)
        {
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
