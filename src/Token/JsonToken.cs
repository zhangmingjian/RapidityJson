using System;

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
            return Parse(json, new JsonOption());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static JsonToken Parse(string json, JsonOption option)
        {
            using (var reader = new JsonReader(json))
            {
                return new JsonSerializer(option).Deserialize<JsonToken>(reader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object To(Type type)
        {
            return new JsonSerializer().Deserialize(this, type);
        }

        public virtual object To(Type type, JsonOption option)
        {
            return new JsonSerializer(option).Deserialize(this, type);
        }

        public virtual T To<T>()
        {
            return To<T>(new JsonOption());
        }

        public virtual T To<T>(JsonOption option)
        {
            return new JsonSerializer(option).Deserialize<T>(this);
        }

        public override string ToString()
        {
            var option = new JsonOption
            {
                SkipValidated = true
            };
            return ToString(option);
        }

        public string ToString(JsonOption option)
        {
            return new JsonSerializer(option).Serialize(this);
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