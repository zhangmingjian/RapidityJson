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

        private TypeConverterProvider _converterProvider;

        public TypeConverterProvider ConverterProvider
        {
            get => _converterProvider = _converterProvider ?? new DefaultTypeConverterProvider();
            set => _converterProvider = value;
        }

        /// <summary>
        /// 对象循环引用处理方式
        /// </summary>
        public LoopReferenceProcess LoopReferenceProcess { get; set; }

        private LoopReferenceChecker _loopReferenceChecker;

        /// <summary>
        /// 
        /// </summary>
        public LoopReferenceChecker LoopReferenceChecker
        {
            get => _loopReferenceChecker = _loopReferenceChecker ?? new LoopReferenceChecker();
            set => _loopReferenceChecker = value;
        }

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
    }
}