using System;
using System.Diagnostics;
using System.IO;

namespace Rapidity.Json.JsonPath
{
    internal class CharScanner
    {
        private ReadOnlyMemory<char> _memory;
        private ReadOnlySpan<char> _span => _memory.Span;
        private int _cursor;

        public int Line;
        public int Position;
        public char Value => _span[_cursor];
        public bool End => _cursor == _span.Length - 1;

        public CharScanner(string str) : this(str.ToCharArray())
        {
        }

        public CharScanner(ReadOnlyMemory<char> memory)
        {
            _memory = memory;
            Line = 1;
            Position = 0;
            _cursor = -1;
        }

        public bool Next()
        {
            if (_cursor >= _span.Length - 1) return false;
            _cursor++;
            Debug.Write(Value);
            switch (Value)
            {
                case JsonConstants.Space:
                case JsonConstants.Tab:
                case JsonConstants.CarriageReturn: Position++; break;
                case JsonConstants.LineFeed:
                    Line++;
                    Position = 0;
                    break;
                default: Position++; break;
            }
            return true;
        }

        public bool Next(params char[] skips)
        {
            if (Next())
            {
                if (Array.IndexOf(skips, Value) > -1)
                    return Next(skips);
            }
            return false;
        }

        public char? Peek()
        {
            if (_cursor >= _span.Length - 1) return default;
            return _span[_cursor + 1];
        }

        public char[] Slice(int start) => _span.Slice(start).ToArray();

        public char[] Slice(int start, int length) => _span.Slice(start, length).ToArray();
    }
}