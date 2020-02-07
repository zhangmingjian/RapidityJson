using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    public abstract class TypeConverterProvider
    {
        public abstract IReadOnlyCollection<IConverterCreator> ConverterFactories { get; }

        public abstract void AddConverterFactory(IConverterCreator converter);

        public abstract TypeConverter Build(Type type);
    }

    internal class DefaultTypeConverterProvider : TypeConverterProvider
    {
        private static ConcurrentDictionary<Type, TypeConverter> _dictionary = new ConcurrentDictionary<Type, TypeConverter>();

        private List<IConverterCreator> _converters;

        public override IReadOnlyCollection<IConverterCreator> ConverterFactories => _converters;

        public DefaultTypeConverterProvider()
        {
            _converters = new List<IConverterCreator>();
        }
        public override void AddConverterFactory(IConverterCreator converter)
        {
            if (_converters.Exists(x => x.GetType() == converter.GetType()))
                throw new JsonException($"集合中已存在{converter.GetType()}类型的{nameof(IConverterCreator)}");
            _converters.Add(converter);
        }

        public override TypeConverter Build(Type type)
        {
            return _dictionary.GetOrAdd(type, Find);
        }

        private TypeConverter Find(Type type)
        {
            TypeConverter convert = null;
            foreach (var creator in ConverterFactories)
            {
                if (creator.CanConvert(type))
                {
                    convert = creator.Create(type, this);
                    break;
                }
            }
            if (convert == null) throw new JsonException($"不支持的类型{type},无法创建{type}的{nameof(TypeConverter)}");
            return convert;
        }
    }
}
