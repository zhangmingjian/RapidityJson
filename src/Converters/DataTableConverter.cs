using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class DataTableConverter : TypeConverterBase, IConverterCreator
    {
        public DataTableConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(DataTable);
        }

        public ITypeConverter Create(Type type)
        {
            return new DataTableConverter(type);
        }

        public override object FromElement(JsonElement element, JsonOption option)
        {
            return new DataTable();
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            return new DataTable();
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            var table = obj as DataTable;
            if (table == null) return;
            writer.WriteStartArray();
            foreach (DataRow row in table.Rows)
            {
                writer.WriteStartObject();
                foreach (DataColumn column in table.Columns)
                {
                    var name = column.ColumnName;
                    var value = row[name];
                    writer.WritePropertyName(name);
                    var convert = option.ConverterProvider.Build(column.DataType);
                    convert.ToWriter(writer, value, option);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
