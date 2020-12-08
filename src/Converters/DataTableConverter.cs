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
            switch (element.ElementType)
            {
                case JsonElementType.Array:
                    var table = new DataTable();
                    foreach (var ele in (JsonArray)element)
                    {
                        if (ele.ElementType != JsonElementType.Object) throw new JsonException($"不支持转换为DataRow类型，ElementType:{ele.ElementType}");
                        var jObj = (JsonObject)ele;
                        FillDataTable(jObj, table, option);
                    }
                    return table;
                case JsonElementType.Null: return null;
                default: throw new JsonException($"不支持转换为DataTable类型，ElementType:{element.ElementType}");
            }
        }

        private void FillDataTable(JsonObject jObj, DataTable table, JsonOption option)
        {
            if (table.Rows.Count == 0) //首行需要添加列信息
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                var convert = option.ConverterProvider.Build(typeof(object));
                foreach (var property in jObj)
                {
                    var name = property.Key;
                    object value = convert.FromElement(property.Value, option);

                    table.Columns.Add(name, value?.GetType() ?? typeof(object));
                    dic[name] = value;
                }
                var row = table.NewRow();
                foreach (var item in dic)
                {
                    row[item.Key] = item.Value;
                }
                table.Rows.Add(row);
            }
            else
            {
                var row = table.NewRow();
                var convert = option.ConverterProvider.Build(typeof(object));
                foreach (var property in jObj)
                {
                    object value = convert.FromElement(property.Value, option);

                    row[property.Key] = value;
                }
                table.Rows.Add(row);
            }
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.StartArray:
                    var table = new DataTable();
                    FillDataTable(reader, table, option);
                    return table;
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"不支持转换为DataTable类型，JsonTokenType:{reader.TokenType}");
            }
        }

        private void FillDataTable(JsonReader reader, DataTable table, JsonOption option)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        var convert = option.ConverterProvider.Build(typeof(Dictionary<string, object>));
                        var dic = convert.FromReader(reader, option) as Dictionary<string, object>;
                        if (table.Rows.Count == 0)
                        {
                            foreach (var keypair in dic)
                            {
                                table.Columns.Add(keypair.Key, keypair.Value?.GetType() ?? typeof(object));
                            }
                        }
                        var row = table.NewRow();
                        foreach (var keypair in dic)
                        {
                            row[keypair.Key] = keypair.Value;
                        }
                        table.Rows.Add(row);
                        break;
                    case JsonTokenType.EndArray: return;
                    default: throw new JsonException($"不支持转换为DataTable类型，JsonTokenType:{reader.TokenType}");
                }
            }
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
                    //循环引用处理
                    if (HandleLoopReferenceValue(writer, name, value, option))
                        continue;
                    writer.WritePropertyName(name);
                    base.ToWriter(writer, value, option);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            option.LoopReferenceChecker.PopObject();
        }
    }

    internal class DataSetConverter : TypeConverterBase, IConverterCreator
    {
        public DataSetConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(DataSet);
        }

        public ITypeConverter Create(Type type)
        {
            return new DataSetConverter(type);
        }

        public override object FromElement(JsonElement element, JsonOption option)
        {
            switch (element.ElementType)
            {
                case JsonElementType.Object:
                    var jObj = (JsonObject)element;
                    var dataset = new DataSet();
                    var convert = option.ConverterProvider.Build(typeof(DataTable));
                    foreach (var property in (JsonObject)element)
                    {
                        var table = convert.FromElement(property.Value, option) as DataTable;
                        if (table != null)
                        {
                            table.TableName = property.Key;
                            dataset.Tables.Add(table);
                        }
                    }
                    return dataset;
                case JsonElementType.Null: return null;
                default: throw new JsonException($"不支持转换为DataSet类型，ElementType:{element.ElementType}");
            }
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.StartObject:
                    var dataset = new DataSet();
                    FillDataSet(reader, dataset, option);
                    return dataset;
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"不支持转换为DataSet类型，JsonTokenType:{reader.TokenType}");
            }
        }

        private void FillDataSet(JsonReader reader, DataSet dataSet, JsonOption option)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        var name = reader.Text;
                        reader.Read();
                        var convert = option.ConverterProvider.Build(typeof(DataTable));
                        var table = convert.FromReader(reader, option) as DataTable;
                        if (table == null) table = new DataTable();
                        table.TableName = name;
                        dataSet.Tables.Add(table);
                        break;
                    case JsonTokenType.EndObject: return;
                    default: throw new JsonException($"不支持转换为DataSet类型，JsonTokenType:{reader.TokenType}");
                }
            }
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            var dataset = (DataSet)obj;
            writer.WriteStartObject();
            foreach (DataTable table in dataset.Tables)
            {
                var name = table.TableName ?? "";
                //循环引用处理
                if (HandleLoopReferenceValue(writer, name, table, option))
                    continue;
                writer.WritePropertyName(name);
                base.ToWriter(writer, table, option);
            }
            writer.WriteEndObject();
            option.LoopReferenceChecker.PopObject();
        }
    }
}
