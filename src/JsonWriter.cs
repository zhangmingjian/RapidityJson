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
        private string _indeteChars; //缩进字符
        private char _quoteSymbol = JsonConstants.Quote;   //引号字符
        private int _depth;
        private JsonTokenType _tokenType;
        public JsonTokenType TokenType => _tokenType;
        public int Depth => _depth;
        private Stack<JsonTokenType> _tokens;
        private JsonContainerType _containerType;

        public JsonWriter(TextWriter writer) : this(writer, new JsonOption())
        {
        }

        public JsonWriter(TextWriter writer, JsonOption option)
        {
            _writer = writer;
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _tokens = new Stack<JsonTokenType>();
            if (_option.Indented)
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

        public void WriteElement(JsonElement element)
        {
            switch (element.ElementType)
            {
                case JsonElementType.Object: WriteObject((JsonObject)element); break;
                case JsonElementType.Array: WriteArray((JsonArray)element); break;
                case JsonElementType.String: WriteString((JsonString)element); break;
                case JsonElementType.Number: WriteNumber((JsonNumber)element); break;
                case JsonElementType.Boolean: WriteBoolean((JsonBoolean)element); break;
                case JsonElementType.Null: WriteNull(); break;
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
            WriteElement(property.Value);
        }

        public void WriteArray(JsonArray token)
        {
            WriteStartArray();
            foreach (var item in token)
                WriteElement(item);
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
            ValidateNext(JsonTokenType.StartObject);
            WriteComma();
            _writer.Write(JsonConstants.OpenBrace);
            _depth++;
            WriteIndented();
            _tokenType = JsonTokenType.StartObject;
        }

        public void WriteEndObject()
        {
            ValidateNext(JsonTokenType.EndObject);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBrace);
            _tokenType = JsonTokenType.EndObject;
        }

        public void WriteStartArray()
        {
            ValidateNext(JsonTokenType.StartArray);
            WriteComma();
            _writer.Write(JsonConstants.OpenBracket);
            _depth++;
            WriteIndented();
            _tokenType = JsonTokenType.StartArray;
        }

        public void WriteEndArray()
        {
            ValidateNext(JsonTokenType.EndArray);
            _depth--;
            WriteIndented();
            _writer.Write(JsonConstants.CloseBracket);
            _tokenType = JsonTokenType.EndArray;
        }

        /// <summary>
        /// write property name
        /// </summary>
        /// <param name="name"></param>
        public void WritePropertyName(string name)
        {
            ValidateNext(JsonTokenType.PropertyName);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(name);
            _writer.Write(_quoteSymbol);
            _writer.Write(JsonConstants.Colon);
            _tokenType = JsonTokenType.PropertyName;
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }
            ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(value);
            _writer.Write(_quoteSymbol);
            _tokenType = JsonTokenType.String;
        }

        private void WriteEscapeString(string value)
        {
            for (int i = 0; i < value.Length; i++)
                WriteEscapeString(value[i]);
        }

        private void WriteEscapeString(char value)
        {
            switch (value)
            {
                case '\\': _writer.Write(@"\\"); break;         //反斜杠
                case '\r': _writer.Write(@"\r"); break;         //回车(CR) ，将当前位置移到本行开头
                case '\n': _writer.Write(@"\n"); break;         //换行(LF) ，将当前位置移到下一行开头
                case '\t': _writer.Write(@"\t"); break;         //水平制表(HT) （跳到下一个TAB位置）
                case '\b': _writer.Write(@"\b"); break;         //退格(BS) ，将当前位置移到前一列
                case '\f': _writer.Write(@"\f"); break;         //换页(FF)，将当前位置移到下页开头
                case '\a': _writer.Write(@"\u0007"); break;     //响铃(BEL)
                case '\v': _writer.Write(@"\u000b"); break;     //垂直制表(VT)
                case '\u0085': _writer.Write(@"\u0085"); break;   //下一行
                case '\u2028': _writer.Write(@"\u2028"); break;   //行分隔符
                case '\u2029': _writer.Write(@"\u2029"); break;   //段落分隔符
                case '\0': _writer.Write(@"\u0000"); break;
                case '\uffff': _writer.Write(@"\uffff"); break;
                default:
                    if (value == _quoteSymbol) _writer.Write('\\');
                    _writer.Write(value);
                    break;
            }
        }

        public void WriteDateTime(DateTime value)
        {
            ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString(_option.DateTimeFormat) + _quoteSymbol);
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString(_option.DateTimeFormat) + _quoteSymbol);
            _tokenType = JsonTokenType.String;
        }

        public void WriteGuid(Guid value)
        {
            ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol + value.ToString() + _quoteSymbol);
            _tokenType = JsonTokenType.String;
        }

        public void WriteChar(char value)
        {
            ValidateNext(JsonTokenType.String);
            WriteComma();
            _writer.Write(_quoteSymbol);
            WriteEscapeString(value);
            _writer.Write(_quoteSymbol);
            _tokenType = JsonTokenType.String;
        }

        public void WriteInt(int value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteUInt(uint value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteLong(long value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteULong(ulong value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteFloat(float value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            if (float.IsNaN(value))
                _writer.Write(_quoteSymbol + JsonConstants.NaN + _quoteSymbol);
            else if (float.IsNegativeInfinity(value))
                _writer.Write(_quoteSymbol + JsonConstants.NegativeInfinity + _quoteSymbol);
            else if (float.IsPositiveInfinity(value))
                _writer.Write(_quoteSymbol + JsonConstants.PositiveInfinity + _quoteSymbol);
            else _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteDouble(double value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            if (double.IsNaN(value))
                _writer.Write(_quoteSymbol + JsonConstants.NaN + _quoteSymbol);
            else if (double.IsNegativeInfinity(value))
                _writer.Write(_quoteSymbol + JsonConstants.NegativeInfinity + _quoteSymbol);
            else if (double.IsPositiveInfinity(value))
                _writer.Write(_quoteSymbol + JsonConstants.PositiveInfinity + _quoteSymbol);
            else _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteDecimal(decimal value)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(value);
            _tokenType = JsonTokenType.Number;
        }

        private void WriteNumber(string number)
        {
            ValidateNext(JsonTokenType.Number);
            WriteComma();
            _writer.Write(number);
            _tokenType = JsonTokenType.Number;
        }

        public void WriteBoolean(bool value)
        {
            var token = value ? JsonTokenType.True : JsonTokenType.False;
            ValidateNext(token);
            WriteComma();
            _writer.Write(value ? JsonConstants.TrueString : JsonConstants.FalseString);
            _tokenType = token;
        }

        public void WriteNull()
        {
            ValidateNext(JsonTokenType.Null);
            WriteComma();
            _writer.Write(JsonConstants.NullString);
            _tokenType = JsonTokenType.Null;
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
            if (_option.Indented)
            {
                _writer.WriteLine();
                for (var i = 0; i < _depth; i++)
                    _writer.Write(_indeteChars);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        private void ValidateNext(JsonTokenType next)
        {
            if (_option.SkipValidated) return;
            switch (_tokenType)
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
                case JsonTokenType.Null:
                    switch (next)
                    {
                        case JsonTokenType.PropertyName:
                            if (_containerType == JsonContainerType.Object)
                            {
                                _tokens.Push(next);
                                return;
                            }
                            break;
                        case JsonTokenType.StartObject:
                        case JsonTokenType.StartArray:
                            if (_containerType == JsonContainerType.Array)
                            {
                                _tokens.Push(next);
                                return;
                            }
                            break;
                        case JsonTokenType.EndObject:
                            if (_containerType == JsonContainerType.Object)
                            {
                                PopToken(next, JsonTokenType.StartObject);
                                return;
                            }
                            break;
                        case JsonTokenType.EndArray:
                            if (_containerType == JsonContainerType.Array)
                            {
                                PopToken(next, JsonTokenType.StartArray);
                                return;
                            }
                            break;
                        case JsonTokenType.String:
                        case JsonTokenType.Number:
                        case JsonTokenType.True:
                        case JsonTokenType.False:
                        case JsonTokenType.Null:
                            if (_containerType == JsonContainerType.Array) return;
                            break;
                        default: break;
                    }
                    ThrowException(next);
                    break;
                case JsonTokenType.Comment: break;
            }
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
                    ThrowException(JsonTokenType.None, next);
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
                    ThrowException(JsonTokenType.StartObject, next);
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
                    ThrowException(JsonTokenType.PropertyName, next);
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
                    ThrowException(JsonTokenType.StartArray, next);
                    break;
            }
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
            throw new JsonException($"无效的JSON Token，{next}不能出现在{current ?? _tokenType}之后");
        }

        public void Dispose()
        {
            _writer?.Close();
            _writer?.Dispose();
        }
    }
}