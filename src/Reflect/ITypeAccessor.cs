using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rapidity.Json.Reflect
{
    internal interface ITypeAccessor
    {
        public Type Type { get; }
        public object CreateInstance();
        public object CreateInstance(params object[] args);

        public IMemberAccessor GetProperty(string name);
        public IMemberAccessor GetProperty(string name, BindingFlags flags);

        public IMemberAccessor GetField(string name);
        public IMemberAccessor GetField(string name, BindingFlags flags);

        public IEnumerable<IMemberAccessor> GetMembers();
        public IEnumerable<IMemberAccessor> GetMembers(BindingFlags flags);

        public IMethodAccessor GetMethod(string name);
        public IMethodAccessor GetMethod(string name, params Type[] parameters);
        public IMethodAccessor GetMethod(string name, Type[] parameters, BindingFlags flags);
    }
}