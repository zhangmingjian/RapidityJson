using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ObjectKeyValueConverter : DictionaryConverter
    {
        public ObjectKeyValueConverter(Type type, TypeConverterProvider provider)
                : base(type, typeof(object), typeof(object), provider)
        {
        }
    }

    internal class StringKeyValueConverter : DictionaryConverter
    {
        public StringKeyValueConverter(Type type, TypeConverterProvider provider)
            : base(type, typeof(string), typeof(string), provider)
        {
        }
    }
}
