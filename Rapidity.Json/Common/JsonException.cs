using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonException : Exception
    {
        public int Line { get; set; }

        public int Position { get; set; }

        public JsonException()
        {
        }

        public JsonException(string message) : base(message)
        {
        }

        public JsonException(string message, int line, int position) : base($"{message},行 {line},列 {position}")
        {
            this.Line = line;
            this.Position = position;
        }
    }
}