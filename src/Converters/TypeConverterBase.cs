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

        protected virtual bool HandleLoopReferenceValue(JsonWriter writer, object value, JsonOption option)
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
        protected virtual bool HandleLoopReferenceValue(JsonWriter writer, string propertyName, object value, JsonOption option)
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
