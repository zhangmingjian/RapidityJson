using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    public class JsonOption
    {
        /// <summary>
        /// 最大深度
        /// </summary>
        public int MaxDepth { get; set; } = 32;
        /// <summary>
        /// 是否忽略默认值
        /// </summary>
        public bool IgnoreDefalutValue { get; set; }
        /// <summary>
        /// 是否忽略null值
        /// </summary>
        public bool IgnoreNullValue { get; set; }

        public static JsonOption Defalut => new JsonOption
        {
        };
    }

    public struct JsonWriteOption
    {
        /// <summary>
        /// 是否缩进
        /// </summary>
        public bool Indented { get; set; }
        /// <summary>
        ///缩进字符数
        /// </summary>
        public int IndenteLength { get; set; }
        /// <summary>
        /// DateTime格式FormatString
        /// </summary>
        public string DateTimeFormat { get; set; }
        /// <summary>
        /// 关闭 验证写入的token,默认开启验证
        /// </summary>
        public bool SkipValidated { get; set; }
        /// <summary>
        /// 属性/字符串-使用单引号
        /// </summary>
        public bool UseSingleQuote { get; set; }

        public static JsonWriteOption Default => new JsonWriteOption
        {
            Indented = true,
            SkipValidated = true
        };
    }
}