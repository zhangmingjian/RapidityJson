using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonWriter : IDisposable
    {
        private TextWriter _writer;
        private JsonOption _option;
        private TokenValidator _tokenValidator;
        private string _indeteChars; //缩进字符
        private char _quoteSymbol = JsonConstants.Quote;   //引号字符
        private int _depth;
        private JsonTokenType _tokenType;
        public JsonTokenType TokenType => _tokenType;
        public int Depth => _depth;

        public JsonWriter(TextWriter writer) : this(writer, new JsonOption())
        {
        }

        public JsonWriter(TextWriter writer, JsonOption option)
        {
            _writer = writer;
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _tokenValidator = _option.SkipValidated ? TokenValidator.None : TokenValidator.Default;
            if (_option.Indented || _option.IndenteLength > 0)
            {
                if (_option.IndenteLength <= 0) _indeteChars = JsonConstants.Tab.ToString();
                else
                {
                    for (int i = 0; i < _option.IndenteLength; i++)
                        _indeteChars = string.Concat(_indeteChars, JsonConstants.Space);
                }
            }
        }

        #region write for jsontoken

        public void WriteToken(JsonToken token)
        {
            switch (token.ValueType)
            {
                case JsonValueType.Object: WriteObject((JsonObject)token); break;
                case JsonValueType.Array: WriteArray((JsonArray)token); break;
                case JsonValueType.String: WriteString((JsonString)token); break;
                case JsonValueType.Number: WriteNumber((JsonNumber)token); break;
                case JsonValueType.Boolean: WriteBoolean((JsonBoolean)token); break;
                case JsonValueType.Null: WriteNull(); break;
                default: break;
            }
        }

        public void WriteObject(JsonObject token)
        {
            WriteStartObject();
            foreach (var property in token.GetAllProperty())
                WriteProperty(property);
            WriteEndObject();
        }

        public void WriteProperty(JsonProperty property)
        {
            WritePropertyName(property.Name);
            WriteToken(property.Value);
        }

        public void WriteArray(JsonArray token)
        {
            WriteStartArray();
            foreach (var item in token)
                WriteToken(item);
            WriteEndArray();
        }

        public void WriteString(JsonString token)
        {
            WriteString(token.Value);
        }

        public void WriteBoolean(JsonBoolean token)
        {
            WriteBoolean(token.Value);
        }

        public void WriteNumber(JsonNumber token)
        {
            WriteNumber(token.ToString());
        }

        #endregion

        public void WriteStartObject()
        {
            _tokenValidator.ValidateNext(JsonTokenType.StartObject);
            WriteComma();
            _writer.Write(JsonConstants.OpenBrace);
            _depth++;
            WriteIndented();
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteEndObject()
        {
            _tokenValidator.ValidateNext(JsonTokenType.EndObject);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBrace);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteStartArray()
        {
            _tokenValidator.ValidateNext(JsonTokenType.StartArray);
            WriteComma();
            _writer.Write(JsonConstants.OpenBracket);
            _depth++;
            WriteIndented();
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteEndArray()
        {
            _tokenValidator.ValidateNext(JsonTokenType.EndArray);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBracket);
            _tokenType = _tokenValidator.TokenType;
        }

        /// <summary>
        /// write property name
        /// </summary>
        /// <param name="name"></param>
        public void WritePropertyName(string name)
        {
            _tokenValidator.ValidateNext(JsonTokenType.PropertyName);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(name);
            _writer.Write(_quoteSymbol);
            _writer.Write(JsonConstants.Colon);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }
            _tokenValidator.ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(value);
            _writer.Write(_quoteSymbol);
            _tokenType = _tokenValidator.TokenType;
        }

        private void WriteEscapeString(string value)
        {
            foreach (var @char in value)
            {
                switch (@char)
                {
                    case '\a': _writer.Write(JsonConstants.BELChars); break;    //响铃(BEL)
                    case '\b': _writer.Write("\\b"); break;  //退格(BS) ，将当前位置移到前一列
                    case '\r': _writer.Write("\\r"); break;  //回车(CR) ，将当前位置移到本行开头
                    case '\f': _writer.Write("\\f"); break;  //换页(FF)，将当前位置移到下页开头
                    case '\n': _writer.Write("\\n"); break;  //换行(LF) ，将当前位置移到下一行开头
                    case '\t': _writer.Write("\\t"); break;  //水平制表(HT) （跳到下一个TAB位置）
                    case '\v': _writer.Write(JsonConstants.VTChars); break;     //垂直制表(VT)
                    case '\\': _writer.Write("\\\\"); break;                    //反斜杠
                    default:
                        if (@char == _quoteSymbol) _writer.Write('\\');
                        _writer.Write(@char);
                        break;
                }
            }
        }

        public void WriteDateTime(DateTime value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString(_option.DateTimeFormat) + _quoteSymbol);
            _tokenType = _tokenValidator.TokenType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString(_option.DateTimeFormat) + _quoteSymbol);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteGuid(Guid value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString() + _quoteSymbol);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteChar(char value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value + _quoteSymbol);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteInt(int value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteUInt(uint value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteLong(long value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteULong(ulong value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteFloat(float value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            if (float.IsNaN(value) || float.IsNegativeInfinity(value) || float.IsPositiveInfinity(value))
                _writer.Write(_quoteSymbol + value.ToString() + _quoteSymbol);
            else _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteDouble(double value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            if (double.IsNaN(value) || double.IsNegativeInfinity(value) || double.IsPositiveInfinity(value))
                _writer.Write(_quoteSymbol + value.ToString() + _quoteSymbol);
            else _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteDecimal(decimal value)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = _tokenValidator.TokenType;
        }

        private void WriteNumber(string number)
        {
            _tokenValidator.ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(number);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteBoolean(bool value)
        {
            _tokenValidator.ValidateNext(value ? JsonTokenType.True : JsonTokenType.False);
            WriteComma();
            _writer.Write(value ? JsonConstants.TrueString : JsonConstants.FalseString);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteNull()
        {
            _tokenValidator.ValidateNext(JsonTokenType.Null);
            WriteComma();
            _writer.Write(JsonConstants.NullString);
            _tokenType = _tokenValidator.TokenType;
        }

        public void WriteRaw(string raw)
        {
            using (var read = new JsonReader(raw))
            {
                while (read.Read())
                {
                    switch (read.TokenType)
                    {
                        case JsonTokenType.StartObject: WriteStartObject(); break;
                        case JsonTokenType.StartArray: WriteStartArray(); break;
                        case JsonTokenType.EndObject: WriteEndObject(); break;
                        case JsonTokenType.EndArray: WriteEndArray(); break;
                        case JsonTokenType.PropertyName: WritePropertyName(read.Text); break;
                        case JsonTokenType.String: WriteString(read.Text); break;
                        case JsonTokenType.Number: WriteNumber(read.Text); break;
                        case JsonTokenType.True: WriteBoolean(true); break;
                        case JsonTokenType.False: WriteBoolean(false); break;
                        case JsonTokenType.Null: WriteNull(); break;
                    }
                }
            }
        }

        /// <summary>
        /// 添加逗号
        /// </summary>
        private void WriteComma()
        {
            switch (_tokenType)
            {
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Null:
                    _writer.Write(JsonConstants.Comma);
                    WriteIndented();
                    break;
            }
        }

        /// <summary>
        /// 添加缩进
        /// </summary>
        private void WriteIndented()
        {
            if ((_indeteChars ?? string.Empty).Length > 0)
            {
                _writer.WriteLine();
                for (var i = 0; i < _depth; i++)
                    _writer.Write(_indeteChars);
            }
        }

        public void Dispose()
        {
            _writer?.Close();
            _writer?.Dispose();
        }
    }

    /// <summary>
    /// 验证写入token的合法性
    /// </summary>
    internal class TokenValidator
    {
        /// <summary>
        /// 不验证token合法性
        /// </summary>
        public static TokenValidator None => new TokenValidator();
        /// <summary>
        /// 默认验证器
        /// </summary>
        public static TokenValidator Default => new DefaultTokenValidator();

        public JsonTokenType TokenType = JsonTokenType.None;

        public virtual void ValidateNext(JsonTokenType next)
        {
            TokenType = next;
        }

        /// <summary>
        /// 验证状态 默认实现
        /// </summary>
        private class DefaultTokenValidator : TokenValidator
        {
            private Stack<JsonTokenType> _tokens;
            private JsonContainerType _containerType;

            public DefaultTokenValidator()
            {
                _tokens = new Stack<JsonTokenType>();
                _containerType = JsonContainerType.None;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="next"></param>
            public override void ValidateNext(JsonTokenType next)
            {
                switch (TokenType)
                {
                    case JsonTokenType.None: ValidateStart(next); break;
                    case JsonTokenType.StartObject: ValidateStartObject(next); break;
                    case JsonTokenType.PropertyName: ValidatePropery(next); break;
                    case JsonTokenType.StartArray: ValidateStartArray(next); break;
                    case JsonTokenType.EndObject:
                    case JsonTokenType.EndArray:
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null: ValidateToken(next); break;
                    case JsonTokenType.Comment: break;
                }
                TokenType = next;
            }

            private void ValidateStart(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.StartObject:
                        _containerType = JsonContainerType.Object;
                        _tokens.Push(next);
                        break;
                    case JsonTokenType.StartArray:
                        _containerType = JsonContainerType.Array;
                        _tokens.Push(next);
                        break;
                    case JsonTokenType.PropertyName:
                    case JsonTokenType.EndObject:
                    case JsonTokenType.EndArray:
                        ThrowException(next);
                        break;
                    default: break;
                }
            }

            private void ValidateStartObject(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.PropertyName:
                        _tokens.Push(next);
                        break;
                    case JsonTokenType.EndObject:
                        PopToken(next, JsonTokenType.StartObject);
                        break;
                    default:
                        ThrowException(next);
                        break;
                }
            }

            private void ValidatePropery(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.StartObject:
                        _tokens.Push(next);
                        _containerType = JsonContainerType.Object;
                        break;
                    case JsonTokenType.StartArray:
                        _tokens.Push(next);
                        _containerType = JsonContainerType.Array;
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Null:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Number:
                        PopToken(next, JsonTokenType.PropertyName);
                        break;
                    default:
                        ThrowException(next);
                        break;
                }
            }

            private void ValidateStartArray(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.StartObject:
                        _tokens.Push(next);
                        _containerType = JsonContainerType.Object;
                        break;
                    case JsonTokenType.StartArray:
                        _tokens.Push(next);
                        _containerType = JsonContainerType.Array;
                        break;
                    case JsonTokenType.EndArray:
                        PopToken(next, JsonTokenType.StartArray);
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Null:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Number:
                        break;
                    default:
                        ThrowException(next);
                        break;
                }
            }

            private void ValidateToken(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.PropertyName:
                        if (_containerType == JsonContainerType.Object)
                        {
                            _tokens.Push(next);
                            return;
                        }
                        //ThrowException(next);
                        break;
                    case JsonTokenType.StartObject:
                    case JsonTokenType.StartArray:
                        if (_containerType == JsonContainerType.Array)
                        {
                            _tokens.Push(next);
                            return;
                        }
                        //ThrowException(next);
                        break;
                    case JsonTokenType.EndObject:
                        if (_containerType == JsonContainerType.Object)
                        {
                            PopToken(next, JsonTokenType.StartObject);
                            return;
                        }
                        //ThrowException(next);
                        break;
                    case JsonTokenType.EndArray:
                        if (_containerType == JsonContainerType.Array)
                        {
                            PopToken(next, JsonTokenType.StartArray);
                            return;
                        }
                        //ThrowException(next);
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        if (_containerType == JsonContainerType.Array) return;
                        //ThrowException(next);
                        break;
                    default: break;
                }
                ThrowException(next);
            }

            private void PopToken(JsonTokenType next, JsonTokenType topToken)
            {
                if (_tokens.Count > 0)
                {
                    var top = _tokens.Pop(); //栈顶值必须与toptoken一致
                    if (top == topToken)
                    {
                        if (_tokens.Count > 0 && _tokens.Peek() == JsonTokenType.PropertyName) //上一个是propertyName时继续出栈
                            _tokens.Pop();
                        if (_tokens.Count > 0) _containerType = _tokens.Peek() == JsonTokenType.StartArray ? JsonContainerType.Array : JsonContainerType.Object;
                        else _containerType = JsonContainerType.None;
                        return;
                    }
                    ThrowException(next, top);
                }
                ThrowException(next);
            }

            private void ThrowException(JsonTokenType next, JsonTokenType? current = null)
            {
                throw new JsonException($"无效的JSON Token，{next}不能出现在{current ?? TokenType}之后");
            }
        }
    }
}