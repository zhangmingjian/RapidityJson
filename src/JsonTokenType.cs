using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    /// <summary>
    /// json标识类型
    /// </summary>
    public enum JsonTokenType : byte
    {
        None,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        PropertyName,
        String,
        Number,
        True,
        False,
        Null,
        Comment
    }

    /// <summary>
    /// json值类型
    /// </summary>
    public enum JsonElementType : byte
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null,
        Comment
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum JsonContainerType : byte { None, Object, Array }
}
