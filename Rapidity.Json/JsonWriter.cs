using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonWriter : IDisposable
    {
        private TextWriter _writer;
        private JsonWriteOption _option;
        private TokenValidator _tokenValidator;
        private int _depth;
        private string _indeteFilled;
        private char _quoteSymbol;
        public JsonTokenType CurrentToken { get; private set; }

        public JsonWriter(TextWriter writer) : this(writer, default)
        {
        }

        public JsonWriter(TextWriter writer, JsonWriteOption option)
        {
            _writer = writer;
            _option = option;
            _tokenValidator = _option.SkipValidated ? TokenValidator.None : TokenValidator.Default;
            _quoteSymbol = _option.UseSingleQuote ? JsonConstants.SingleQuote : JsonConstants.Quote;
            if (_option.Indented || _option.IndenteLength > 0)
            {
                if (_option.IndenteLength <= 0) _indeteFilled = JsonConstants.Tab.ToString();
                else
                {
                    for (int i = 0; i < _option.IndenteLength; i++)
                        _indeteFilled = string.Concat(_indeteFilled, JsonConstants.Space);
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
            _tokenValidator.Validate(JsonTokenType.StartObject);
            WriteComma();
            _writer.Write(JsonConstants.OpenBrace);
            _depth++;
            WriteIndented();
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteEndObject()
        {
            _tokenValidator.Validate(JsonTokenType.EndObject);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBrace);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteStartArray()
        {
            _tokenValidator.Validate(JsonTokenType.StartArray);
            WriteComma();
            _writer.Write(JsonConstants.OpenBracket);
            _depth++;
            WriteIndented();
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteEndArray()
        {
            _tokenValidator.Validate(JsonTokenType.EndArray);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBracket);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        /// <summary>
        /// write property name
        /// </summary>
        /// <param name="name"></param>
        public void WritePropertyName(string name)
        {
            _tokenValidator.Validate(JsonTokenType.PropertyName);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(name);
            _writer.Write(_quoteSymbol);
            _writer.Write(JsonConstants.Colon);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteString(string value)
        {
            _tokenValidator.Validate(JsonTokenType.String);
            WriteComma();
            if (value == null) WriteNull();
            else
            {
                _writer.Write(_quoteSymbol);
                WriteEscapeString(value);
                _writer.Write(_quoteSymbol);
            }
            CurrentToken = _tokenValidator.CurrentToken;
        }

        private void WriteEscapeString(string value)
        {
            foreach (var @char in value + "")
            {
                switch (@char)
                {
                    case '\a': _writer.Write(JsonConstants.BELChars); break;    //响铃(BEL)
                    case '\b': _writer.Write('\\'); _writer.Write('b'); break;  //退格(BS) ，将当前位置移到前一列
                    case '\f': _writer.Write('\\'); _writer.Write('f'); break;  //换页(FF)，将当前位置移到下页开头
                    case '\r': _writer.Write('\\'); _writer.Write('r'); break;  //回车(CR) ，将当前位置移到本行开头
                    case '\n': _writer.Write('\\'); _writer.Write('n'); break;  //换行(LF) ，将当前位置移到下一行开头
                    case '\t': _writer.Write('\\'); _writer.Write('t'); break;  //水平制表(HT) （跳到下一个TAB位置）
                    case '\v': _writer.Write(JsonConstants.VTChars); break;     //垂直制表(VT)
                    case '\\': _writer.Write('\\'); _writer.Write('\\'); break;
                    default:
                        if (@char == _quoteSymbol) _writer.Write('\\');
                        _writer.Write(@char);
                        break;
                }
            }
        }

        public void WriteDateTime(DateTime value)
        {
            _tokenValidator.Validate(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString(_option.DateTimeFormat, CultureInfo.CurrentCulture) + _quoteSymbol);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteGuid(Guid value)
        {
            _tokenValidator.Validate(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString() + _quoteSymbol);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteChar(char value)
        {
            _tokenValidator.Validate(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value + _quoteSymbol);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteInt(int value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteUInt(uint value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteLong(long value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteULong(ulong value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteFloat(float value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteDouble(double value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteDecimal(decimal value)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        private void WriteNumber(string number)
        {
            _tokenValidator.Validate(JsonTokenType.Number);
            WriteComma();
            _writer.Write(number);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteBoolean(bool value)
        {
            _tokenValidator.Validate(value ? JsonTokenType.True : JsonTokenType.False);
            WriteComma();
            _writer.Write(value ? JsonConstants.TrueString : JsonConstants.FalseString);
            CurrentToken = _tokenValidator.CurrentToken;
        }

        public void WriteNull()
        {
            _tokenValidator.Validate(JsonTokenType.Null);
            WriteComma();
            _writer.Write(JsonConstants.NullString);
            CurrentToken = _tokenValidator.CurrentToken;
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
                        case JsonTokenType.PropertyName: WritePropertyName(read.Value); break;
                        case JsonTokenType.String: WriteString(read.Value); break;
                        case JsonTokenType.Number: WriteNumber(read.Value); break;
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
            switch (CurrentToken)
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
            if ((_indeteFilled ?? string.Empty).Length > 0)
            {
                _writer.WriteLine();
                for (var i = 0; i < _depth; i++)
                    _writer.Write(_indeteFilled);
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

        public JsonTokenType CurrentToken { get; private set; } = JsonTokenType.None;

        public virtual void Validate(JsonTokenType next)
        {
            CurrentToken = next;
        }

        public virtual void Validate()
        {
        }

        /// <summary>
        /// 验证状态 默认实现
        /// </summary>
        private class DefaultTokenValidator : TokenValidator
        {

            private enum InDocument : byte { None, Object, Array }

            private Stack<JsonTokenType> _tokens;
            private InDocument _inDocument;

            public DefaultTokenValidator()
            {
                _tokens = new Stack<JsonTokenType>();
                _inDocument = InDocument.None;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void Validate()
            {
                if (_tokens.Count >= 0)
                {
                    var token = _tokens.Peek();
                    if (token == JsonTokenType.StartArray)
                        throw new JsonException("Invalid JSON format, missing symbol ']'");
                    else
                        throw new JsonException("Invalid JSON format, missing symbol '}'");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="next"></param>
            public override void Validate(JsonTokenType next)
            {
                switch (CurrentToken)
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
                    case JsonTokenType.Null: ValidateEndToken(next); break;

                    case JsonTokenType.Comment: break;
                }
                CurrentToken = next;
            }

            private void ValidateStart(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.StartObject:
                        _inDocument = InDocument.Object;
                        _tokens.Push(next);
                        break;
                    case JsonTokenType.StartArray:
                        _inDocument = InDocument.Array;
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
                        PopToken(JsonTokenType.StartObject, next);
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
                        _inDocument = InDocument.Object;
                        break;
                    case JsonTokenType.StartArray:
                        _tokens.Push(next);
                        _inDocument = InDocument.Array;
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Null:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Number:
                        PopToken(JsonTokenType.PropertyName, next);
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
                        _inDocument = InDocument.Object;
                        break;
                    case JsonTokenType.StartArray:
                        _tokens.Push(next);
                        _inDocument = InDocument.Array;
                        break;
                    case JsonTokenType.EndArray:
                        PopToken(JsonTokenType.StartArray, next);
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

            private void ValidateEndToken(JsonTokenType next)
            {
                switch (next)
                {
                    case JsonTokenType.PropertyName:
                        if (_inDocument == InDocument.Object)
                        {
                            _tokens.Push(next);
                            break;
                        }
                        ThrowException(next);
                        break;
                    case JsonTokenType.StartObject:
                    case JsonTokenType.StartArray:
                        if (_inDocument == InDocument.Array)
                        {
                            _tokens.Push(next);
                            break;
                        }
                        ThrowException(next);
                        break;
                    case JsonTokenType.EndObject:
                        if (_inDocument == InDocument.Object)
                        {
                            PopToken(JsonTokenType.StartObject, next);
                            break;
                        }
                        ThrowException(next);
                        break;
                    case JsonTokenType.EndArray:
                        if (_inDocument == InDocument.Array)
                        {
                            PopToken(JsonTokenType.StartArray, next);
                            break;
                        }
                        ThrowException(next);
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        if (_inDocument == InDocument.Array) break;
                        ThrowException(next);
                        break;
                    case JsonTokenType.Comment: break;
                    default:
                        ThrowException(next);
                        break;
                }
            }

            private void PopToken(JsonTokenType previous, JsonTokenType next)
            {
                if (_tokens.Count > 0 && _tokens.Peek() == previous)
                {
                    var peek = _tokens.Peek();
                    if (peek == previous)
                    {
                        _tokens.Pop();
                        if (_tokens.Count > 0 && _tokens.Peek() == JsonTokenType.PropertyName)
                            _tokens.Pop();
                        if (_tokens.Count > 0)
                            _inDocument = _tokens.Peek() == JsonTokenType.StartObject ? InDocument.Object : InDocument.Array;
                        else
                            _inDocument = InDocument.None;
                        return;
                    }
                    ThrowException(next, peek);
                }
                ThrowException(next);
            }

            private void ThrowException(JsonTokenType next, JsonTokenType? current = null)
            {
                throw new JsonException($"Invalid JsonToken:{next}, after JsonToken:{current ?? CurrentToken}");
            }
        }
    }

    internal class Utf8StringWriter : StringWriter
    {
        private Encoding _encoding;
        public override Encoding Encoding => _encoding;

        public Utf8StringWriter(Encoding encoding = null) : this(new StringBuilder(1024), CultureInfo.CurrentCulture, encoding)
        {
        }

        public Utf8StringWriter(StringBuilder sb, IFormatProvider provider, Encoding encoding = null) : base(sb, provider)
        {
            _encoding = encoding ?? Encoding.UTF8;
        }
    }
}