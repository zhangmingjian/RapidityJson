using Rapidity.Json.Converters;

namespace Rapidity.Json
{
    public class JsonOption
    {
        /// <summary>
        /// 最大深度
        /// </summary>
        public int MaxDepth { get; set; } = 32;
        /// <summary>
        /// 是否忽略null值
        /// </summary>
        public bool IgnoreNullValue { get; set; }
        /// <summary>
        /// 属性驼峰命名
        /// </summary>
        public bool CamelCaseNamed { get; set; }

        /// <summary>
        /// 默认序列化写枚举的name,开启后写枚举value
        /// </summary>
        public bool WriteEnumValue { get; set; }

        #region  JsonWriter Options
        /// <summary>
        /// 是否缩进
        /// </summary>
        public bool Indented { get; set; }
        /// <summary>
        ///缩进字符数
        /// </summary>
        public int IndenteLength { get; set; }
        /// <summary>
        /// 关闭 验证写入的token,默认开启验证
        /// </summary>
        public bool SkipValidated { get; set; }
        /// <summary>
        /// DateTime格式FormatString
        /// </summary>
        public string DateTimeFormat { get; set; }

        #endregion

        private TypeConverterProvider _converterFactory;

        public TypeConverterProvider ConverterFactory
        {
            get => _converterFactory = _converterFactory ?? new DefaultTypeConverterProvider();
            set => _converterFactory = value;
        }
    }
}