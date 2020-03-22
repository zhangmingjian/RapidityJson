using System.Collections.Generic;

namespace Rapidity.Json
{
    internal static class JsonConstants
    {
        public const char OpenBrace = '{';
        public const char CloseBrace = '}';
        public const char OpenBracket = '[';
        public const char CloseBracket = ']';
        public const char Space = ' ';
        public const char Comma = ',';
        public const char Colon = ':';
        public const char Quote = '"';
        public const char SingleQuote = '\'';
        public const char BackSlash = '\\';

        /// <summary>
        /// 回车符
        /// </summary>
        public const char CarriageReturn = '\r';
        /// <summary>
        /// 换行符
        /// </summary>
        public const char LineFeed = '\n';
        public const char Tab = '\t';
        public const char T = 't';
        public const char F = 'f';
        public const char N = 'n';
        public const char Plus = '+';
        public const char Hyphen = '-';
        /// <summary>
        /// //读取结束标志
        /// </summary>
        public const int End = -1;

        public const string TrueString = "true";
        public const string FalseString = "false";
        public const string NullString = "null";

        public const string NaN = "NaN";
        public const string PositiveInfinity = "Infinity";
        public const string NegativeInfinity = "-Infinity";
    }
}