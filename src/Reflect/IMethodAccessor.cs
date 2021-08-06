using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Reflect
{
    public interface IMethodAccessor
    {
        string Name { get; }
        MethodInfo MethodInfo { get; } 
        object Invoke(object instance, params object[] args);
    }
}
