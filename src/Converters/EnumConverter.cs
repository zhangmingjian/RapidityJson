﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class EnumConverter : TypeConverterBase, IConverterCreator
    {
        public EnumConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            return type.IsEnum;
        }

        public ITypeConverter Create(Type type)
        {
            return new EnumConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader,option);
                case JsonTokenType.Number:
                case JsonTokenType.String:
                    if (TryParse(reader.Text, out object value)) return value;
                    throw new JsonException($"无效的Enum:{Type},值：{reader.Text}", reader.Line, reader.Position);
                default:
                    throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        private bool TryParse(string value, out object obj)
        {
            try
            {
                obj = Enum.Parse(Type, value, true);
                return Enum.IsDefined(Type, obj);
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        public override object FromElement(JsonElement element, JsonOption option)
        {
            switch (element.ElementType)
            {
                case JsonElementType.Number:
                    var numberToken = (JsonNumber)element;
                    if (numberToken.TryGetInt(out int num)) return num;
                    throw new JsonException($"无效的{Type}值：{numberToken.ToString()},{nameof(EnumConverter)}反序列化{Type}失败");
                case JsonElementType.String:
                    var strToken = (JsonString)element;
                    if (TryParse(strToken.Value, out object val)) return val;
                    throw new JsonException($"无效的{Type}值：{strToken.Value},{nameof(EnumConverter)}反序列化{Type}失败");
            }
            throw new JsonException($"无法从{element.ElementType}转换为{Type},{nameof(EnumConverter)}反序列化{Type}失败");
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (option.WriteEnumValue) writer.WriteInt((int)obj);
            else writer.WriteString(obj.ToString());
        }
    }
}
