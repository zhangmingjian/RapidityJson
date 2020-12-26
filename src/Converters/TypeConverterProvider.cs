using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rapidity.Json.Converters
{
    public abstract class TypeConverterProvider
    {
        public abstract IEnumerable<IConverterCreator> GetConverterCreators();

        public abstract void AddConverterFactory(IConverterCreator converter);

        public virtual ITypeConverter Build(Type type)
        {
            ITypeConverter convert = null;
            foreach (var creator in GetConverterCreators())
            {
                if (creator.CanConvert(type))
                {
                    convert = creator.Create(type);
                    break;
                }
            }
            if (convert == null) throw new JsonException($"创建{type}的{nameof(ITypeConverter)}失败，不支持的类型:{type.FullName}");
            return convert;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DefaultTypeConverterProvider : TypeConverterProvider
    {
        private static Dictionary<Type, ITypeConverter> _dictionary = new Dictionary<Type, ITypeConverter>();

        private LinkedList<IConverterCreator> _converters;

        public override IEnumerable<IConverterCreator> GetConverterCreators() => _converters;

        public DefaultTypeConverterProvider()
        {
            _converters = new LinkedList<IConverterCreator>(new List<IConverterCreator> {
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
                new JsonElementConverter(null),
                new DataSetConverter(null),
                new DataTableConverter(null),
                new ObjectConverter(null),
            });
        }
        public override void AddConverterFactory(IConverterCreator converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (_converters.Any(x => x.GetType() == converter.GetType()))
                throw new JsonException($"集合中已存在{converter.GetType()}类型的{nameof(IConverterCreator)}");
            _converters.AddFirst(converter);
        }

        public override ITypeConverter Build(Type type)
        {
            if (!_dictionary.ContainsKey(type))
                _dictionary[type] = base.Build(type);
            return _dictionary[type];
        }
    }
}