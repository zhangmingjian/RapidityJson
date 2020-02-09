using System;
using System.Collections.Generic;
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
        private StringBuilder _buffer;
        private JsonOption _option;
        private int _line;          //行号
        private int _position;      //列号
        private ReadState _state;   //读取状态
        private JsonContainerType _containerType;   //所在容器类型
        private Stack<JsonTokenType> _tokens;
        private char _quoteSymbol;  //引号类型
        private char _currentChar;  //当前字符
        private int _depth;         //当前深度
        private JsonTokenType _tokenType; //当前jsontoken
        private string _text; //当前读取的文本值
        private double? _number; //当前读取的数字值

        #region public properties
        /// <summary>
        /// 行号
        /// </summary>
        public int Line => _line;
        /// <summary>
        /// 列号
        /// </summary>
        public int Position => _position;
        /// <summary>
        /// 当前字符
        /// </summary>
        public char CurrentChar => _currentChar;
        /// <summary>
        /// 当前深度
        /// </summary>
        public int Depth => _depth;
        /// <summary>
        /// 当前jsontoken
        /// </summary>
        public JsonTokenType TokenType => _tokenType;
        /// <summary>
        /// 读取的文本值
        /// </summary>
        public string Text => _text;
        /// <summary>
        /// 读取的数字值
        /// </summary>
        public double? Number => _number;
        #endregion

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
            _buffer = new StringBuilder(64);
            _containerType = JsonContainerType.None;
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
        private char MoveNext(bool skipEmpty = false)
        {
            bool canRead;
            do
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
            } while (canRead);
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
            _containerType = JsonContainerType.Object;
            _depth++;
            if (_depth > _option.MaxDepth) throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// EndObject
        /// </summary>
        /// <returns></returns>
        private bool EndObject()
        {
            PopToken(JsonTokenType.StartObject);
            _state = ReadState.EndObject;
            SetToken(JsonTokenType.EndObject);
            _depth--;
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
            _containerType = JsonContainerType.Array;
            _depth++;
            if (_depth > _option.MaxDepth) throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EndArray()
        {
            PopToken(JsonTokenType.StartArray);
            _state = ReadState.EndArray;
            SetToken(JsonTokenType.EndArray);
            _depth--;
            return true;
        }
        /// <summary>
        /// 读取属性节点
        /// </summary>
        /// <returns></returns>
        private bool ReadProperty()
        {
            MoveNext(true);
            switch (_currentChar)
            {
                case JsonConstants.Quote:
                case JsonConstants.SingleQuote:
                    _quoteSymbol = _currentChar;
                    var property = ReadStringToBuffer();
                    //属性后面只能是冒号：
                    if (MoveNext(true) != JsonConstants.Colon)
                        throw new JsonException($"非法字符{_currentChar}", _line, _position);
                    _state = ReadState.Property;
                    SetToken(JsonTokenType.PropertyName, property);
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
        /// 该方法的调用只有三种情况，1.JSON开始；2.Property后； 3.StartArray后；
        /// </summary>
        /// <returns></returns>
        private bool ReadToken()
        {
            MoveNext(true);
            switch (_currentChar)
            {
                case JsonConstants.Quote:
                case JsonConstants.SingleQuote: //读取字符串
                    TryPopProperty();
                    _quoteSymbol = _currentChar;
                    var buffer = ReadStringToBuffer();
                    SetToken(JsonTokenType.String, buffer);
                    _state = ReadState.Value;
                    return true;
                case JsonConstants.OpenBrace: return StartObject();
                case JsonConstants.OpenBracket: return StartArray();
                //case JsonConstants.CloseBrace: return StartArray(); //当前方法调用不允许出现 “}”
                case JsonConstants.CloseBracket:
                    //属性或逗号后面不允许 ]
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
            MoveNext(true);
            switch (_currentChar)
            {
                case JsonConstants.Comma: //逗号之后，依据ContainerType值判断分支
                    _state = ReadState.Comma;
                    if (_containerType == JsonContainerType.Object) return ReadProperty();
                    if (_containerType == JsonContainerType.Array) return ReadToken();
                    throw new JsonException($"非法字符{_currentChar}", _line, _position);
                case JsonConstants.CloseBrace: return EndObject();
                case JsonConstants.CloseBracket: return EndArray();
                case JsonConstants.End: return ValidateEndToken();
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
            throw new JsonException($"JSON未正常结束", _line, _position);
        }

        /// <summary>
        /// 读取/验证string
        /// </summary>
        /// <returns></returns>
        private string ReadStringToBuffer()
        {
            bool canRead = true;
            do
            {
                switch (MoveNext())
                {
                    case JsonConstants.BackSlash:
                        switch (MoveNext())
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
                    case JsonConstants.SingleQuote: //遇到引号结束
                        canRead = _currentChar != _quoteSymbol;
                        if (canRead) _buffer.Append(_currentChar);
                        break;
                    case JsonConstants.End: throw new JsonException($"没有结束标识{_quoteSymbol}", _line, _position);
                    default: _buffer.Append(_currentChar); break;
                }
            } while (canRead);
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
            TryPopProperty();
            for (int i = 1; i < target.Length; i++)
            {
                if (MoveNext() != target[i])
                    throw new JsonException($"非法字符[{_currentChar}]", _line, _position);
            }
            _state = ReadState.Value;
            SetToken(targetToken);
            return true;
        }
        /// <summary>
        /// 读取number
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private bool ReadNumber()
        {
            TryPopProperty();
            _buffer.Append(_currentChar);
            var canRead = true;
            do
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
                    case '-': _buffer.Append(MoveNext()); break;
                    case JsonConstants.Comma:
                    case JsonConstants.CloseBrace:
                    case JsonConstants.CloseBracket:
                    case JsonConstants.Space:
                    case JsonConstants.CarriageReturn:
                    case JsonConstants.LineFeed:
                    case JsonConstants.Tab:
                    case JsonConstants.End: canRead = false; break;
                    default: throw new JsonException($"无效的JSON Number {_buffer.Append(next)}", _line, _position);
                }
            } while (canRead);
            var value = _buffer.ToString();
            if (double.TryParse(value, out double number))
            {
                _buffer.Length = 0;
                _state = ReadState.Value;
                SetToken(JsonTokenType.Number, value, number);
                return true;
            }
            throw new JsonException($"无效的JSON Number {value}", _line, _position);
        }

        #endregion

        #region token validated

        private void SetToken(JsonTokenType token, string text = null, double? number = null)
        {
            _tokenType = token;
            _text = text;
            _number = number;
        }

        /// <summary>
        /// PopToken出栈
        /// </summary>
        /// <param name="previousToken"></param>
        private void PopToken(JsonTokenType previousToken)
        {
            if (_tokens.Count > 0 && _tokens.Peek() == previousToken)
            {
                _tokens.Pop();
                //出栈后栈顶是PropertyName时需要继续出栈
                if (_tokens.Count > 0 && _tokens.Peek() == JsonTokenType.PropertyName)
                    _tokens.Pop();
                if (_tokens.Count > 0)
                    _containerType = _tokens.Peek() == JsonTokenType.StartArray ? JsonContainerType.Array : JsonContainerType.Object;
                else _containerType = JsonContainerType.None;
                return;
            }
            throw new JsonException($"无效字符{_currentChar}", _line, _position);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TryPopProperty()
        {
            if (_containerType == JsonContainerType.Object)
                PopToken(JsonTokenType.PropertyName);
        }

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

    internal enum JsonContainerType : byte { None, Object, Array }
}