using System;

namespace Rapidity.Json.Converters
{
    public interface IConverterCreator
    {
        bool CanConvert(Type type);

        ITypeConverter Create(Type type);
    }
}
