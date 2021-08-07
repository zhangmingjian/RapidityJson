using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapidity.Json.Reflect
{
    internal class TypeAccessor : ITypeAccessor
    {
        private static readonly ConcurrentDictionary<TypeKey, TypeAccessor> _typeCache = new ConcurrentDictionary<TypeKey, TypeAccessor>();
        protected readonly ConcurrentDictionary<int, IMethodAccessor> _methodCache = new ConcurrentDictionary<int, IMethodAccessor>();
        protected readonly ConcurrentDictionary<string, IMemberAccessor> _memberCache = new ConcurrentDictionary<string, IMemberAccessor>();
        protected readonly ConcurrentDictionary<int, IEnumerable<IMemberAccessor>> _membersCache = new ConcurrentDictionary<int, IEnumerable<IMemberAccessor>>();

        public Type Type { get; }

        public TypeAccessor(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Type = type;
        }

        #region 创建实例

        private Func<object> _ctorFactory;
        /// <summary>
        /// 使用默认无参构造函数创建，如果没有默认构造函数，则找最少参数的去构造
        /// </summary>
        /// <returns></returns>
        protected virtual Func<object> CtorFactory()
        {
            NewExpression newExp;
            //查找参数最少的一个构造函数
            var constructors = Type.GetConstructors().OrderBy(t => t.GetParameters().Length);
            if (constructors.Count() == 0)
                newExp = Expression.New(Type);
            else
            {
                var constructor = constructors.First();
                var parameters = constructor.GetParameters();
                List<Expression> parametExps = new List<Expression>();
                foreach (var para in parameters)
                {
                    //有参构造函数使用默认值填充
                    var defaultValue = para.ParameterType.IsValueType ? Activator.CreateInstance(para.ParameterType) : null;
                    ConstantExpression constant = Expression.Constant(defaultValue);
                    var paraValueExp = Expression.Convert(constant, para.ParameterType);
                    parametExps.Add(paraValueExp);
                }
                newExp = Expression.New(constructor, parametExps);
            }

            var body = Type.IsValueType
                            ? Expression.Convert(newExp, typeof(object))
                            : Expression.TypeAs(newExp, typeof(object));

            Expression<Func<object>> expression = Expression.Lambda<Func<object>>(body);
            return expression.Compile();
        }

        public virtual object CreateInstance()
        {
            if (_ctorFactory == null) _ctorFactory = CtorFactory();
            return _ctorFactory();
        }

        private Func<object[], object> _ctorArgsFactory;
        protected virtual Func<object[], object> CtorArgsFactory(object[] args)
        {
            var types = args.Select(x => x?.GetType() ?? null).ToArray();
            var construct = Type.GetConstructor(types);
            if (construct == null) throw new Exception($"未找到包含{args.Length}个参数类型对应的构造函数");

            var argExps = new List<Expression>();
            var paraExp = Expression.Parameter(typeof(object[]), "arg");
            var conParas = construct.GetParameters();
            for (int i = 0; i < conParas.Length; i++)
            {
                var param = conParas[i];
                var propExp = Expression.ArrayIndex(paraExp, Expression.Constant(i));
                var argExp = param.ParameterType.IsValueType
                                ? Expression.Convert(propExp, param.ParameterType)
                                : Expression.TypeAs(propExp, param.ParameterType);
                argExps.Add(argExp);
            }
            NewExpression body = Expression.New(construct, argExps);
            return Expression.Lambda<Func<object[], object>>(body, paraExp).Compile();
        }

        public virtual object CreateInstance(params object[] args)
        {
            if (_ctorArgsFactory == null) _ctorArgsFactory = CtorArgsFactory(args);
            return _ctorArgsFactory(args);
        }

        #endregion

        #region 属性访问

        public virtual IMemberAccessor GetProperty(string name)
        {
            return GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        public virtual IMemberAccessor GetProperty(string name, BindingFlags flags)
        {
            if (_memberCache.ContainsKey(name)) return _memberCache[name];
            var property = FindProperty(Type, name, flags);
            if (property == null) return null;
            var accessor = new PropertyAccessor(property);
            _memberCache.TryAdd(property.Name, accessor);
            return accessor;
        }

        private PropertyInfo FindProperty(Type type, string name, BindingFlags flags)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null) return property;

            return type.GetProperties(flags).FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region 字段访问

        public virtual IMemberAccessor GetField(string name)
        {
            return GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        public virtual IMemberAccessor GetField(string name, BindingFlags flags)
        {
            if (_memberCache.ContainsKey(name)) return _memberCache[name];
            var property = FindField(Type, name, flags);
            if (property == null) return null;
            var accessor = new FieldAccessor(property);
            _memberCache.TryAdd(name, accessor);
            return accessor;
        }

        private FieldInfo FindField(Type type, string name, BindingFlags flags)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var field = type.GetField(name, flags);
            if (field != null) return field;

            return type.GetFields(flags).FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        #endregion

        #region 方法访问

        public IMethodAccessor GetMethod(string name)
        {
            return GetMethod(name, Type.EmptyTypes);
        }

        public IMethodAccessor GetMethod(string name, params Type[] parameters)
        {
            return GetMethod(name, parameters, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public IMethodAccessor GetMethod(string name, Type[] parameters, BindingFlags flags)
        {
            int key = MethodAccessor.GetKey(name, parameters);
            if (_methodCache.ContainsKey(key)) return _methodCache[key];
            var info = FindMethod(Type, name, parameters, flags);
            if (info == null) return null;
            var accessor = new MethodAccessor(info);
            _methodCache.TryAdd(key, accessor);
            return accessor;
        }

        private MethodInfo FindMethod(Type type, string name, Type[] parameterTypes, BindingFlags flags)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (parameterTypes == null)
                parameterTypes = Type.EmptyTypes;

            //first try full match
            var methodInfo = type.GetMethod(name, flags, null, CallingConventions.Any, parameterTypes, null);
            if (methodInfo != null) return methodInfo;

            // next, get all that match by name
            var methodsByName = type.GetMethods(flags)
              .Where(m => m.Name == name)
              .ToList();

            if (methodsByName.Count == 0)
                return null;

            // if only one matches name, return it
            if (methodsByName.Count == 1)
                return methodsByName.FirstOrDefault();

            // next, get all methods that match param count
            var methodsByParamCount = methodsByName
              .Where(m => m.GetParameters().Length == parameterTypes.Length)
              .ToList();

            // if only one matches with same param count, return it
            if (methodsByParamCount.Count == 1)
                return methodsByParamCount.FirstOrDefault();

            // still no match, make best guess by greatest matching param types
            MethodInfo current = methodsByParamCount.FirstOrDefault();
            int matchCount = 0;

            foreach (var info in methodsByParamCount)
            {
                var paramTypes = info.GetParameters()
                  .Select(p => p.ParameterType)
                  .ToArray();

                // unsure which way IsAssignableFrom should be checked?
                int count = paramTypes
                  .Where((t, i) => t.IsAssignableFrom(parameterTypes[i]))
                  .Count();

                if (count <= matchCount)
                    continue;

                current = info;
                matchCount = count;
            }

            return current;
        }

        #endregion

        public IMemberAccessor GetMember(string name)
        {
            return GetProperty(name) ?? GetField(name);
        }

        public IMemberAccessor GetMember(string name, BindingFlags flags)
        {
            return GetProperty(name, flags) ?? GetField(name, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public IMemberAccessor GetMember(MemberInfo member)
        {
            if (member.DeclaringType != Type) throw new Exception($"当前member:{member.Name}不属于{Type.Name}");
            if (_memberCache.ContainsKey(member.Name)) return _memberCache[member.Name];
            IMemberAccessor accessor = null;
            if (member.MemberType == MemberTypes.Property) accessor = new PropertyAccessor((PropertyInfo)member);
            else if (member.MemberType == MemberTypes.Field) accessor = new FieldAccessor((FieldInfo)member);
            else return null;
            _memberCache.TryAdd(member.Name, accessor);
            return accessor;
        }

        public IEnumerable<IMemberAccessor> GetMembers()
        {
            return GetMembers(BindingFlags.Public | BindingFlags.Instance);
        }

        public IEnumerable<IMemberAccessor> GetMembers(BindingFlags flags)
        {
            var key = (int)flags;
            if (_membersCache.ContainsKey(key)) return _membersCache[key];

            var list = new List<IMemberAccessor>();
            var properties = Type.GetProperties(flags);
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var accessor = new PropertyAccessor(property);
                list.Add(accessor);
                _memberCache.TryAdd(property.Name, accessor);
            }
            var fields = Type.GetFields(flags);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var accessor = new FieldAccessor(field);
                list.Add(accessor);
                _memberCache.TryAdd(field.Name, accessor);
            }
            _membersCache.TryAdd(key, list);
            return list;
        }

        public static TypeAccessor Build(Type type)
        {
            return _typeCache.GetOrAdd(new TypeKey(type), t => new TypeAccessor(type));
        }

        public static TypeAccessor<T> Build<T>()
        {
            return (TypeAccessor<T>)_typeCache.GetOrAdd(new TypeKey(typeof(T), true), t => new TypeAccessor<T>());
        }

        class TypeKey
        {
            public Type Type { get; private set; }
            public bool Generic { get; private set; }
            public TypeKey(Type type, bool generic = false)
            {
                Type = type;
                Generic = generic;
            }

            /// <summary>
            /// 重写gethashcode同时也要重写equals方法
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return Type.GetHashCode() + Generic.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = (TypeKey)obj;
                return this.Type == other.Type && this.Generic == other.Generic;
            }

            public override string ToString()
            {
                return $"{Type}{(Generic ? $"({nameof(Generic)})" : "")}";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypeAccessor<T> : TypeAccessor
    {
        public TypeAccessor() : base(typeof(T))
        {
        }

        private Func<T> _ctorFactory;

        private new Func<T> CtorFactory()
        {
            return Expression.Lambda<Func<T>>(Expression.New(Type)).Compile();
        }

        public new T CreateInstance()
        {
            if (_ctorFactory == null) _ctorFactory = CtorFactory();
            return _ctorFactory();
        }

        private Func<object[], T> _ctorArgsFactory;

        private new Func<object[], T> CtorArgsFactory(object[] args)
        {
            var types = args.Select(x => x?.GetType() ?? null).ToArray();
            var construct = Type.GetConstructor(types);
            if (construct == null) throw new Exception($"未找到包含{args.Length}个参数类型对应的构造函数");

            var argExps = new List<Expression>();
            var paraExp = Expression.Parameter(typeof(object[]));
            var conParas = construct.GetParameters();
            for (int i = 0; i < conParas.Length; i++)
            {
                var param = conParas[i];
                var propExp = Expression.ArrayIndex(paraExp, Expression.Constant(i));
                var argExp = param.ParameterType.IsValueType
                                ? Expression.Convert(propExp, param.ParameterType)
                                : Expression.TypeAs(propExp, param.ParameterType);
                argExps.Add(argExp);
            }
            NewExpression body = Expression.New(construct, argExps);
            return Expression.Lambda<Func<object[], T>>(body, paraExp).Compile();
        }

        public new T CreateInstance(params object[] args)
        {
            if (_ctorArgsFactory == null) _ctorArgsFactory = CtorArgsFactory(args);
            return _ctorArgsFactory(args);
        }

        public IMemberAccessor Member<TMember>(Expression<Func<T, TMember>> propertyExp)
        {
            if (propertyExp == null) throw new ArgumentNullException(nameof(propertyExp));
            return FindMember(propertyExp.Body as MemberExpression);
        }

        private IMemberAccessor FindMember(MemberExpression memberExp)
        {
            if (memberExp == null) throw new ArgumentNullException("The expression is not a member access expression.", nameof(memberExp));

            if (memberExp.Member is PropertyInfo property)
            {
                return _memberCache.GetOrAdd(property.Name, t => new PropertyAccessor(property));
            }
            if (memberExp.Member is FieldInfo field)
            {
                return _memberCache.GetOrAdd(field.Name, t => new FieldAccessor(field));
            }
            throw new ArgumentException("The member access expression does not access a property or field.", nameof(memberExp));
        }
    }
}
