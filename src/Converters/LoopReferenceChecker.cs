using System;
using System.Collections.Generic;

namespace Rapidity.Json.Converters
{
    public class LoopReferenceChecker
    {
        private readonly Stack<object> stack = new Stack<object>();     

        public virtual void PushRootObject(object obj)
        {
            if (CanPush(obj)) stack.Push(obj); 
        }

        public virtual bool Exsits(object obj)
        {
            if (!CanPush(obj)) return false;
            if (stack.Contains(obj)) return true;
            stack.Push(obj);
            return false;
        }

        public virtual void PopObject()
        {
            stack.Pop();
        }

        protected virtual bool CanPush(object obj)
        {
            if (obj == null || obj.GetType().IsPrimitive
              || obj is string || obj is decimal || obj is DateTime || obj is DateTimeOffset
              || obj is Guid || obj is JsonString || obj is JsonNumber || obj is JsonBoolean
              || obj is JsonNull || obj is DBNull)
                return false;
            return true;
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
