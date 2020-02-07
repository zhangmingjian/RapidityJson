using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    public interface IConverterCreator
    {
        bool CanConvert(Type type);

        TypeConverter Create(Type type, TypeConverterProvider provider);
    }
}
