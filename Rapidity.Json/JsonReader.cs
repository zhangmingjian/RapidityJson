using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonReader : IDisposable
    {
        private TextReader _reader;
        private int _line;          //行号
        private int _position;      //列号
        private ReadState _state;   //
        private InContainer _inContainer;
        private Stack<JsonTokenType> _tokens;
        private char _quoteSymbol;
        private char _currentChar;
        private StringBuilder _buffer;
        private JsonOption _option;
        /// <summary>
        /// 当前深度
        /// </summary>
        public int Depth { get; private set; }
        /// <summary>
        /// 当前tokentype
        /// </summary>
        public JsonTokenType TokenType { get; private set; }
        /// <summary>
        /// 当前Value
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public JsonReader(string json) : this(new StringReader(json), JsonOption.Defalut)
        {
        }

        public JsonReader(string json, JsonOption option) : this(new StringReader(json), option)
        {
        }

        public JsonReader(TextReader reader) : this(reader, JsonOption.Defalut)
        {
        }

        public JsonReader(TextReader reader, JsonOption option)
        {
            _reader = reader;
            _tokens = new Stack<JsonTokenType>();
            _option = option ?? JsonOption.Defalut;
            _buffer = new StringBuilder(1024);
            _inContainer = InContainer.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            switch (_state)
            {
                case ReadState.Start: _line = _position = 1; return ReadToken();
                case ReadState.StartObject: return ReadProperty();
                case ReadState.Property:
                case ReadState.StartArray: return ReadToken();
                case ReadState.EndObject:
                case ReadState.EndArray:
                case ReadState.Comma:
                case ReadState.Value: return ReadNextToken();
                case ReadState.End: return ValidateEndToken();
                default: throw new JsonException($"非法字符{_currentChar}", _line, _position);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skipEmpty"></param>
        /// <returns></returns>
        private char ReadNext(bool skipEmpty = false)
        {
            bool canRead = true;
            while (canRead)
            {
                _currentChar = (char)_reader.Read();
                switch (_currentChar)
                {
                    case JsonConstants.End:
                        _state = ReadState.End;
                        canRead = false;
                        break;
                    case JsonConstants.Space:
                    case JsonConstants.Tab:
                    case JsonConstants.CarriageReturn:
                        _position++;
                        canRead = skipEmpty;
                        break;
                    case JsonConstants.LineFeed:
                        _line++;
                        _position = 0;
                        canRead = skipEmpty;
                        break;
                    default:
                        _position++;
                        canRead = false;
                        break;
                }
            }
            return _currentChar;
        }

        /// <summary>
        /// 查看下一个字符
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            return (char)_reader.Peek();
        }

        #region Parse value

        private bool StartObject()
        {
            _state = ReadState.StartObject;
            SetToken(JsonTokenType.StartObject);
            _tokens.Push(TokenType);
            _inContainer = InContainer.Object;
            Depth++;
            if (Depth > _option.MaxDepth) throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// EndObject
        /// </summary>
        /// <returns></returns>
        private bool EndObject()
        {
            _state = ReadState.EndObject;
            SetToken(JsonTokenType.EndObject);
            PopToken(JsonTokenType.StartObject);
            Depth--;
            return true;
        }
        /// <summary>
        /// StartArray
        /// </summary>
        /// <returns></returns>
        private bool StartArray()
        {
            _state = ReadState.StartArray;
            SetToken(JsonTokenType.StartArray);
            _tokens.Push(TokenType);
            _inContainer = InContainer.Array;
            Depth++;
            if (Depth > _option.MaxDepth) throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EndArray()
        {
            _state = ReadState.EndArray;
            SetToken(JsonTokenType.EndArray);
            PopToken(JsonTokenType.StartArray);
            Depth--;
            return true;
        }
        /// <summary>
        /// 读取属性节点
        /// </summary>
        /// <returns></returns>
        private bool ReadProperty()
        {
            ReadNext(true);
            switch (_currentChar)
            {
                case JsonConstants.Quote:
                case JsonConstants.SingleQuote:
                    _quoteSymbol = _currentChar;
                    _state = ReadState.Property;
                    SetToken(JsonTokenType.PropertyName, ReadStringToBuffer());
                    //属性后面只能是冒号：
                    if (ReadNext(true) != JsonConstants.Colon)
                        throw new JsonException($"非法字符{_currentChar}", _line, _position);
                    _tokens.Push(TokenType);
                    return true;
                case JsonConstants.CloseBrace:
                    if (_state != ReadState.Comma) return EndObject();
                    throw new JsonException($"非法字符{_currentChar}", _line, _position);
                default: throw new JsonException($"非法字符{_currentChar}", _line, _position);
            }
        }
        /// <summary>
        /// 读取token或value值,
        /// 该方法的调用只有三种情况，1.文档开始；2.Property后； 3.StartArray后；
        /// </summary>
        /// <returns></returns>
        private bool ReadToken()
        {
            ReadNext(true);
            switch (_currentChar)
            {
                case JsonConstants.Quote:
                case JsonConstants.SingleQuote: //读取字符串
                    _quoteSymbol = _currentChar;
                    SetToken(JsonTokenType.String, ReadStringToBuffer());
                    _state = ReadState.Value;
                    if (_inContainer == InContainer.Object) PopToken(JsonTokenType.PropertyName);
                    return true;
                case JsonConstants.OpenBrace: return StartObject();
                case JsonConstants.OpenBracket: return StartArray();
                //case JsonConstants.CloseBrace: return StartArray(); //当前方法调用不允许出现 “}”
                case JsonConstants.CloseBracket:
                    //属性或逗号后面时不允许CloseBracket
                    if (_state == ReadState.Property || _state == ReadState.Comma)
                        throw new JsonException($"非法字符{_currentChar}", _line, _position);
                    return EndArray();
                case JsonConstants.T: return ReadExpectValue(JsonConstants.TrueString, JsonTokenType.True);
                case JsonConstants.F: return ReadExpectValue(JsonConstants.FalseString, JsonTokenType.False);
                case JsonConstants.N: return ReadExpectValue(JsonConstants.NullString, JsonTokenType.Null);
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-': return ReadNumber();
                default: throw new JsonException($"非法字符{_currentChar}", _line, _position);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ReadNextToken()
        {
            ReadNext(true);
            switch (_currentChar)
            {
                case JsonConstants.End: return ValidateEndToken();
                case JsonConstants.Comma:
                    _state = ReadState.Comma;
                    if (_inContainer == InContainer.Object) return ReadProperty();
                    if (_inContainer == InContainer.Array) return ReadToken();
                    throw new JsonException($"非法字符{_currentChar}", _line, _position);
                case JsonConstants.CloseBrace: return EndObject();
                case JsonConstants.CloseBracket: return EndArray();
                default: throw new JsonException($"非法字符{_currentChar}", _line, _position);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ValidateEndToken()
        {
            if (_tokens.Count == 0) return false;
            throw new JsonException($"文档未正常结束,当前值{_currentChar}", _line, _position);
        }

        /// <summary>
        /// 读取/验证string
        /// </summary>
        /// <returns></returns>
        private string ReadStringToBuffer()
        {
            bool canRead = true;
            while (canRead)
            {
                switch (ReadNext())
                {
                    case JsonConstants.End: throw new JsonException($"未找到string结束标识\"", _line, _position);
                    case JsonConstants.BackSlash:
                        switch (ReadNext())
                        {
                            case 'a': _buffer.Append('\a'); break;
                            case 'v': _buffer.Append('\v'); break;
                            case 'b': _buffer.Append('\b'); break;
                            case 'f': _buffer.Append('\f'); break;
                            case 't': _buffer.Append('\t'); break;
                            case 'r': _buffer.Append('\r'); break;
                            case 'n': _buffer.Append('\n'); break;
                            case '/':
                            case '\\':
                            case '"':
                            case '\'': _buffer.Append(_currentChar); break;
                            case 'u': //验证是否十六进制数字并转义
                                var chars = new char[4];
                                _reader.Read(chars, 0, 4);
                                if (int.TryParse(new string(chars), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out int val))
                                {
                                    _buffer.Append(Convert.ToChar(val));
                                    _position += 4;
                                    break;
                                }
                                else throw new JsonException($"无效的十六进制字符{new string(chars)}", _line, _position);
                            default: throw new JsonException($"无效的转义字符\\{_currentChar}", _line, _position);
                        }
                        break;
                    case JsonConstants.Quote:
                    case JsonConstants.SingleQuote: //遇到引号 循环结束
                        canRead = _currentChar != _quoteSymbol;
                        if (canRead) _buffer.Append(_currentChar);
                        break;
                    default: _buffer.Append(_currentChar); break;
                }
            }
            var value = _buffer.ToString();
            _buffer.Length = 0;
            return value;
        }

        /// <summary>
        /// 读取预期值
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="targetToken"></param>
        /// <returns></returns>
        private bool ReadExpectValue(string target, JsonTokenType targetToken)
        {
            for (int i = 1; i < target.Length; i++)
            {
                if (ReadNext() != target[i])
                    throw new JsonException($"非法字符[{_currentChar}]", _line, _position);
            }
            _state = ReadState.Value;
            SetToken(targetToken);
            if (_inContainer == InContainer.Object) PopToken(JsonTokenType.PropertyName);
            return true;
        }
        /// <summary>
        /// 读取number
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private bool ReadNumber()
        {
            _buffer.Append(_currentChar);
            var canRead = true;
            while (canRead)
            {
                var next = Peek();
                switch (next)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                    case 'E':
                    case 'e':
                    case '+':
                    case '-': _buffer.Append(ReadNext()); break;
                    case JsonConstants.Comma:
                    case JsonConstants.CloseBrace:
                    case JsonConstants.CloseBracket:
                    case JsonConstants.Space:
                    case JsonConstants.CarriageReturn:
                    case JsonConstants.LineFeed:
                    case JsonConstants.Tab:
                    case JsonConstants.End: canRead = false; break;
                    default: throw new JsonException($"无效的数字格式[{_buffer.Append(next)}]", _line, _position);
                }
            }
            var number = _buffer.ToString();
            if (!double.TryParse(number, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double val))
                throw new JsonException($"无效的数字格式[{number}]", _line, _position);
            _buffer.Length = 0;
            _state = ReadState.Value;
            SetToken(JsonTokenType.Number, number);
            if (_inContainer == InContainer.Object) PopToken(JsonTokenType.PropertyName);
            return true;
        }

        #endregion

        #region token validated

        private void SetToken(JsonTokenType token, string value = "")
        {
            TokenType = token;
            Value = value;
        }

        /// <summary>
        /// PopToken出栈
        /// </summary>
        /// <param name="previousToken"></param>
        private void PopToken(JsonTokenType previousToken)
        {
            if (_tokens.TryPeek(out JsonTokenType token) && token == previousToken)
            {
                _tokens.Pop();
                //出栈后栈顶是PropertyName时需要继续出栈
                if (_tokens.TryPeek(out JsonTokenType propertyToken) && propertyToken == JsonTokenType.PropertyName)
                    _tokens.Pop();
                if (_tokens.Count > 0)
                    _inContainer = _tokens.Peek() == JsonTokenType.StartArray ? InContainer.Array : InContainer.Object;
                else _inContainer = InContainer.None;
                return;
            }
            throw new JsonException($"无效字符{_currentChar}", _line, _position);
        }
        #endregion

        #region Get Value

        public string GetString() => TokenType == JsonTokenType.Null ? null : Value;

        public int GetInt() => int.Parse(Value, CultureInfo.InvariantCulture);

        public uint GetUInt() => uint.Parse(Value, CultureInfo.InvariantCulture);

        public short GetShort() => short.Parse(Value, CultureInfo.InvariantCulture);

        public ushort GetUShort() => ushort.Parse(Value, CultureInfo.InvariantCulture);

        public long GetLong() => long.Parse(Value, CultureInfo.InvariantCulture);

        public ulong GetULong() => ulong.Parse(Value, CultureInfo.InvariantCulture);

        public float GetFloat() => float.Parse(Value, CultureInfo.InvariantCulture);

        public double GetDouble() => double.Parse(Value, CultureInfo.InvariantCulture);

        public decimal GetDecimal() => decimal.Parse(Value, CultureInfo.InvariantCulture);

        public DateTime GetDateTime() => DateTime.Parse(Value, CultureInfo.CurrentCulture);

        public Guid GetGuid() => Guid.Parse(Value);

        #endregion

        public void Dispose()
        {
            _reader?.Close();
            _reader?.Dispose();
        }

        /// <summary>
        /// json read state
        /// </summary>
        enum ReadState : byte { Start, StartObject, EndObject, StartArray, EndArray, Property, Value, Comma, Comment, End }
    }

    internal enum InContainer : byte { None, Object, Array }
}