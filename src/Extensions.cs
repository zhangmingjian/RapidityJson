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

        public static double? TryToDouble(this string str, double? @defalut = null)
        {
            return double.TryParse(str, out double v) ? v : @defalut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static bool? TryToBool(this string str, bool? @default = null)
        {
            if (string.IsNullOrEmpty(str)) return @default;
            switch (str.ToLower())
            {
                case "0": return false;
                case "1": return true;
                case "false": return false;
                case "true": return true;
                default: return @default;
            }
        }
    }
}
