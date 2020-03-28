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
        private TokenState _state;   //读取状态
        private JsonContainerType _containerType;   //当前容器类型
        private Stack<JsonTokenType> _tokens;
        private char _quoteSymbol;  //引号字符
        private int _current;       //当前读取的字符
        private int _depth;         //当前深度
        private JsonTokenType _tokenType; //当前jsontoken
        private string _text;       //当前读取的文本值
        private double? _number;    //当前读取的数字值

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
        public char CurrentChar => (char)_current;
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
        public JsonReader(string json) : this(new StringReader(json), new JsonOption())
        {
        }

        public JsonReader(string json, JsonOption option) : this(new StringReader(json), option)
        {
        }

        public JsonReader(TextReader reader) : this(reader, new JsonOption())
        {
        }

        public JsonReader(TextReader reader, JsonOption option)
        {
            _reader = reader;
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _tokens = new Stack<JsonTokenType>();
            _buffer = new StringBuilder(64);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            switch (_state)
            {
                case TokenState.Start: _line = _position = 1; return ReadToken(false);
                case TokenState.StartObject: return ReadProperty(true);
                case TokenState.PropertyName: return ReadToken(false);
                case TokenState.StartArray: return ReadToken(true);
                case TokenState.EndObject:
                case TokenState.EndArray:
                case TokenState.Value: return ReadNextToken();
                case TokenState.End: return EndToken();
                default: throw new JsonException($"无效的JSON格式，非法字符{CurrentChar}", _line, _position);
            }
        }

        private int MoveNext(bool skipEmpty = false)
        {
            bool canRead;
            do
            {
                _current = _reader.Read();
                switch (_current)
                {
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
                    case JsonConstants.End:
                        _state = TokenState.End;
                        canRead = false;
                        break;
                    default:
                        _position++;
                        canRead = false;
                        break;
                }
            } while (canRead);
            return _current;
        }

        private bool ReadToken(bool allowClosed)
        {
            MoveNext(true);
            switch (_current)
            {
                case JsonConstants.OpenBrace: return StartObject();
                case JsonConstants.OpenBracket: return StartArray();
                case JsonConstants.CloseBracket:
                    if (allowClosed) return EndArray();
                    break;
                case JsonConstants.SingleQuote:
                case JsonConstants.Quote: return ReadString(CurrentChar);
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
            }
            throw new JsonException($"无效的JSON格式,非法字符{CurrentChar}", _line, _position);
        }

        private bool ReadNextToken()
        {
            MoveNext(true);
            switch (_current)
            {
                case JsonConstants.Comma: //逗号之后，依据ContainerType值判断分支
                    if (_containerType == JsonContainerType.Object) return ReadProperty(false);
                    if (_containerType == JsonContainerType.Array) return ReadToken(false);
                    throw new JsonException($"无效的JSON格式,非法字符{CurrentChar}", _line, _position);
                case JsonConstants.CloseBrace: return EndObject();
                case JsonConstants.CloseBracket: return EndArray();
                case JsonConstants.End: return EndToken();
                default: throw new JsonException($"无效的JSON格式,非法字符{CurrentChar}", _line, _position);
            }
        }

        private bool EndToken()
        {
            if (_tokens.Count == 0) return false;
            throw new JsonException("$无效的JSON格式，非正常结束", _line, _position);
        }

        private bool StartObject()
        {
            _state = TokenState.StartObject;
            _tokenType = JsonTokenType.StartObject;
            _containerType = JsonContainerType.Object;
            _tokens.Push(_tokenType);
            _depth++;
            if (_depth > _option.MaxDepth)
                throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// EndObject
        /// </summary>
        /// <returns></returns>
        private bool EndObject()
        {
            PopToken(JsonTokenType.StartObject);
            _state = TokenState.EndObject;
            _tokenType = JsonTokenType.EndObject;
            _depth--;
            return true;
        }
        /// <summary>
        /// StartArray
        /// </summary>
        /// <returns></returns>
        private bool StartArray()
        {
            _state = TokenState.StartArray;
            _tokenType = JsonTokenType.StartArray;
            _containerType = JsonContainerType.Array;
            _tokens.Push(_tokenType);
            _depth++;
            if (_depth > _option.MaxDepth)
                throw new JsonException($"当前深度超出最大限制：{_option.MaxDepth}", _line, _position);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool EndArray()
        {
            PopToken(JsonTokenType.StartArray);
            _state = TokenState.EndArray;
            _tokenType = JsonTokenType.EndArray;
            _depth--;
            return true;
        }

        /// <summary>
        /// 读取属性节点
        /// </summary>
        /// <returns></returns>
        private bool ReadProperty(bool allowClosed)
        {
            MoveNext(true);
            switch (_current)
            {
                case JsonConstants.Quote:
                case JsonConstants.SingleQuote:
                    _quoteSymbol = CurrentChar;
                    ReadStringToBuffer();
                    //属性后面只能是冒号： 
                    MoveNext(true);
                    if (CurrentChar == JsonConstants.Colon)
                    {
                        _state = TokenState.PropertyName;
                        _tokenType = JsonTokenType.PropertyName;
                        _tokens.Push(_tokenType);
                        return true;
                    }
                    throw new JsonException($"无效的JSON格式, PropertyName:{_text}后只能是：非法字符{CurrentChar}", _line, _position);
                case JsonConstants.CloseBrace:
                    if (allowClosed) return EndObject();
                    break;
                case JsonConstants.End: throw new JsonException("$无效的JSON格式，非正常结束", _line, _position);
            }
            throw new JsonException($"非法字符{CurrentChar}，无效的{JsonTokenType.PropertyName}", _line, _position);
        }

        private void PopToken(JsonTokenType previousToken)
        {
            if (_tokens.Count > 0 && _tokens.Pop() == previousToken)
            {
                //出栈后栈顶是PropertyName时继续出栈
                if (_tokens.Count > 0 && _tokens.Peek() == JsonTokenType.PropertyName)
                    _tokens.Pop();
                if (_tokens.Count > 0)
                    _containerType = _tokens.Peek() == JsonTokenType.StartArray ? JsonContainerType.Array : JsonContainerType.Object;
                else _containerType = JsonContainerType.None;
                return;
            }
            throw new JsonException($"无效的JSON格式，缺失token：{previousToken}", _line, _position);
        }

        /// <summary>
        /// 读取预期值
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="targetToken"></param>
        /// <returns></returns>
        private bool ReadExpectValue(string target, JsonTokenType token)
        {
            TryPopProperty();
            for (int i = 1; i < target.Length; i++)
            {
                MoveNext();
                if (CurrentChar != target[i])
                    throw new JsonException($"无效的JSON格式，非法字符{CurrentChar}", _line, _position);
            }
            _state = TokenState.Value;
            _tokenType = token;
            _text = target;
            return true;
        }

        private bool ReadString(char quote)
        {
            TryPopProperty();
            _quoteSymbol = quote;
            ReadStringToBuffer();
            _state = TokenState.Value;
            _tokenType = JsonTokenType.String;
            return true;
        }

        /// <summary>
        /// 读取/验证string
        /// </summary>
        /// <returns></returns>
        private void ReadStringToBuffer()
        {
            bool canRead = true;
            do
            {
                MoveNext();
                switch (_current)
                {
                    case JsonConstants.BackSlash:
                        MoveNext();
                        switch (_current)
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
                            case '\'': _buffer.Append(CurrentChar); break;
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
                            default: throw new JsonException($"无效的转义字符\\{CurrentChar}", _line, _position);
                        }
                        break;
                    case JsonConstants.Quote:
                    case JsonConstants.SingleQuote: //遇到引号-结束
                        canRead = CurrentChar != _quoteSymbol;
                        if (canRead) _buffer.Append(CurrentChar);
                        break;
                    case JsonConstants.End: throw new JsonException($"无效的JSON格式, 没有结束标识:{_quoteSymbol}", _line, _position);
                    default: _buffer.Append(CurrentChar); break;
                }

            } while (canRead);
            _text = _buffer.ToString();
            _buffer.Clear();
        }


        /// <summary>
        /// 读取number
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private bool ReadNumber()
        {
            TryPopProperty();
            _buffer.Append(CurrentChar);
            var canRead = true;
            do
            {
                var next = _reader.Peek();
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
                    case '-':
                        _buffer.Append((char)MoveNext());
                        break;
                    case JsonConstants.Comma:
                    case JsonConstants.CloseBrace:
                    case JsonConstants.CloseBracket:
                    case JsonConstants.Space:
                    case JsonConstants.CarriageReturn:
                    case JsonConstants.LineFeed:
                    case JsonConstants.Tab:
                    case JsonConstants.End:
                        canRead = false;
                        break;
                    default: throw new JsonException($"无效的JSON格式,非法数字：{_buffer.Append((char)next)}", _line, _position);
                }
            } while (canRead);
            var text = _buffer.ToString();
            if (double.TryParse(text, out double number))
            {
                _buffer.Clear();
                _state = TokenState.Value;
                _tokenType = JsonTokenType.Number;
                _text = text;
                _number = number;
                return true;
            }
            throw new JsonException($"无效的JSON格式,非法数字：{text}", _line, _position);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TryPopProperty()
        {
            if (_containerType == JsonContainerType.Object)
                PopToken(JsonTokenType.PropertyName);
        }

        public void Dispose()
        {
            _reader.Close();
            _reader.Dispose();
        }

        /// <summary>
        /// json read state
        /// </summary>
        enum TokenState : byte
        {
            Start, StartObject, EndObject, StartArray, EndArray, PropertyName, Value, Comment, End
        }
    }
}