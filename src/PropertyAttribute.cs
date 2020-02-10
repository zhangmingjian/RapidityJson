using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// JsonProperty Settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute() { }

        public PropertyAttribute(string name)
        {
            this.Name = name;
        }

        public PropertyAttribute(bool ignore)
        {
            this.Ignore = ignore;
        }

        /// <summary>
        /// PropertyName
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 忽略当前属性
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }
    }
}
