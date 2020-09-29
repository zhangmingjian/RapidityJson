using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    internal static class Extensions
    {
        public static int? TryToInt(this string str, int? @defalut = null)
        {
            return int.TryParse(str, out int v) ? v : @defalut;
        }

        public static int ToInt(this string str)
        {
            return int.TryParse(str, out int v) ? v : 0;
        }
    }
}
