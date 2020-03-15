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
        public const char Slash = '/';
        public const char Point = '.';
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
        /// //读取结束标志 char.MaxValue
        /// </summary>
        public const char End = char.MaxValue;

        public const string TrueString = "true";
        public const string FalseString = "false";
        public const string NullString = "null";

        public const string NaN = "NaN";
        public const string PositiveInfinity = "Infinity";
        public const string NegativeInfinity = "-Infinity";

        /// <summary>
        /// /a 响铃(BEL) \u0007
        /// </summary>
        public static char[] BELChars = new char[] { BackSlash, 'u', '0', '0', '0', '7' };
        /// <summary>
        /// /v 垂直制表(VT) \u000b
        /// </summary>
        public static char[] VTChars = new char[] { BackSlash, 'u', '0', '0', '0', 'b' };
    }
}