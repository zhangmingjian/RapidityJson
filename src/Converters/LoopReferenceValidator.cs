using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Rapidity.Json.Converters
{
    public class LoopReferenceValidator
    {
        private readonly Dictionary<Type, SortedSet<int>> _typeDic = new Dictionary<Type, SortedSet<int>>();
        public virtual bool ExsitLoopReference(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                if (type.IsValueType || type == typeof(string))
                    return false;
                var hashCode = obj.GetHashCode();
                if (_typeDic.ContainsKey(type))
                {
                    var hashCodes = _typeDic[type];
                    if (hashCodes.Contains(hashCode))
                        return true;
                    hashCodes.Add(hashCode);
                }
                else
                {
                    var hasCodes = new SortedSet<int>();
                    hasCodes.Add(hashCode);
                    _typeDic.Add(type, hasCodes);
                }
                return false;
            }
            return false;
        }
    }


    /// <summary>
    /// 对象循环引用处理方式
    /// </summary>
    public enum LoopReferenceProcess
    {
        Null,   //输出null值
        Ignore, //不输出
        Error   //抛异常
    }
}
