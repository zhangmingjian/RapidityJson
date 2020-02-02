using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// jsontoken
    /// </summary>
    public abstract class JsonToken
    {
        public abstract JsonValueType ValueType { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonToken Parse(string json)
        {
            return new JsonParser().Parse(json);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract object To(Type type);

        public override string ToString()
        {
            return ToString(JsonWriteOption.Default);
        }

        public string ToString(JsonWriteOption option)
        {
            using (var sw = new StringWriter(new StringBuilder(1024), CultureInfo.InvariantCulture))
            using (var write = new JsonWriter(sw, option))
            {
                write.WriteToken(this);
                return sw.ToString();
            }
        }

        #region 基本类型转换

        public static implicit operator JsonToken(string value)
        {
            if (value == null) return new JsonNull();
            return new JsonString(value);
        }
        public static implicit operator JsonToken(DateTime value) => new JsonString(value);
        public static implicit operator JsonToken(DateTimeOffset value) => new JsonString(value);
        public static implicit operator JsonToken(Guid value) => new JsonString(value);
        public static implicit operator JsonToken(bool value) => new JsonBoolean(value);
        public static implicit operator JsonToken(byte value) => new JsonNumber(value);
        public static implicit operator JsonToken(short value) => new JsonNumber(value);
        public static implicit operator JsonToken(int value) => new JsonNumber(value);
        public static implicit operator JsonToken(long value) => new JsonNumber(value);
        public static implicit operator JsonToken(float value) => new JsonNumber(value);
        public static implicit operator JsonToken(double value) => new JsonNumber(value);
        public static implicit operator JsonToken(decimal value) => new JsonNumber(value);

        #endregion
    }

    /// <summary>
    /// json标识类型
    /// </summary>
    public enum JsonTokenType : byte
    {
        None,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        PropertyName,
        String,
        Number,
        True,
        False,
        Null,
        Comment
    }

    /// <summary>
    /// json值类型
    /// </summary>
    public enum JsonValueType : byte
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null,
        Comment
    }
}