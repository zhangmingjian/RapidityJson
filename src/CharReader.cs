using System;
using System.Diagnostics;
using System.IO;

namespace Rapidity.Json
{
    internal class CharReader
    {
        private ReadOnlyMemory<char> _memory;
        private int _cursor;

        public char Current => _memory.Span[_cursor];
        public int Line;
        public int Position;

        public CharReader(string str) : this(str.ToCharArray())
        {
        }

        public CharReader(ReadOnlyMemory<char> memory)
        {
            _memory = memory;
            Line = 1;
            Position = 0;
            _cursor = -1;
        }

        public bool Move()
        {
            if (_cursor >= _memory.Length - 1) return false;
            _cursor++;
            Trace.Write(Current);
            switch (Current)
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

        public bool Move(params char[] skips)
        {
            if (Move())
            {
                if (Array.IndexOf(skips, Current) > -1)
                    return Move(skips);
            }
            return false;
        }

        public char? Read()
        {
            if (_cursor >= _memory.Length - 1) return default;
            _cursor++;
            Trace.Write(Current);
            switch (Current)
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
            return Current;
        }

        public char? Peek()
        {
            if (_cursor >= _memory.Length - 1) return default;
            return _memory.Span[_cursor + 1];
        }

        public char[] Slice(int start) => _memory.Slice(start).ToArray();

        public char[] Slice(int start, int length) => _memory.Slice(start, length).ToArray();
    }
}