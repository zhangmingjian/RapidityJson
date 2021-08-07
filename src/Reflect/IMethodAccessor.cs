using System.Reflection;

namespace Rapidity.Json.Reflect
{
    internal interface IMethodAccessor
    {
        string Name { get; }
        MethodInfo MethodInfo { get; } 
        object Invoke(object instance, params object[] args);
    }
}
