﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rapidity.Json.Converters
{
    public abstract class TypeConverterProvider
    {
        public abstract IReadOnlyCollection<IConverterCreator> AllConverterFactories();

        public abstract void AddConverterFactory(IConverterCreator converter);

        public virtual ITypeConverter Build(Type type)
        {
            ITypeConverter convert = null;
            foreach (var creator in AllConverterFactories())
            {
                if (creator.CanConvert(type))
                {
                    convert = creator.Create(type);
                    break; 
                }
            }
            if (convert == null) throw new JsonException($"创建{type}的{nameof(ITypeConverter)}失败，不支持的类型");
            return convert;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DefaultTypeConverterProvider : TypeConverterProvider
    {
        private static Dictionary<Type, ITypeConverter> _dictionary = new Dictionary<Type, ITypeConverter>();
        
        private List<IConverterCreator> _converters;

        public override IReadOnlyCollection<IConverterCreator> AllConverterFactories() => _converters;

        public DefaultTypeConverterProvider()
        {
            _converters = new List<IConverterCreator>()
            {
                new StringConverter(null),
                new EnumConverter(null),
                new DateTimeConverter(null),
                new NullableConverter(null),
                new ValueConverter(null),
                new ListConverter(null,null),
                new ArrayConverter(null),
                new DictionaryConverter(null,null,null),
                new KeyValuePairConverter(null,null,null),
                new StringKeyValueConverter(null),
                new ArrayListConverter(null),
                new JsonTokenConverter(null),
                new ObjectConverter(null),
            };
        }
        public override void AddConverterFactory(IConverterCreator converter)
        {
            if (_converters.Exists(x => x.GetType() == converter.GetType()))
                throw new JsonException($"集合中已存在{converter.GetType()}类型的{nameof(IConverterCreator)}");
            _converters.Add(converter);
        }

        public override ITypeConverter Build(Type type)
        {
            if (!_dictionary.ContainsKey(type))
                _dictionary[type] = base.Build(type);
            return _dictionary[type];
        }
    }
}