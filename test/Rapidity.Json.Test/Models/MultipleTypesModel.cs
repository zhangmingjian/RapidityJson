using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Test.Models
{
    public class MultipleTypesModel
    {

        public ValueModel Single { get; set; }

        public StructModel StructModel { get; set; }

        public IEnumerable<ValueModel> List { get; set; }

        public ValueModel[] Array { get; set; }

        public IDictionary<string, ValueModel> Dictionary { get; set; }

        //[Property(Ignore = true)]
        public IEnumerable<KeyValuePair<int, ValueModel>> KeyValuePairs { get; set; }

    }

    public class ValueModel
    {
        public string StringValue { get; set; }
        public char CharValue { get; set; }
        public char? NullCharValue { get; set; }
        public byte ByteValue { get; set; }
        public byte? NullByteValue { get; set; }
        public short ShortValue { get; set; }
        public short? NullShortValue { get; set; }
        public ushort UShortValue { get; set; }
        public ushort? NullUShortValue { get; set; }
        public int IntValue { get; set; }
        public int? NullIntValue { get; set; }
        public uint UintValue { get; set; }
        public uint? NullUintValue { get; set; }
        public long LongValue { get; set; }
        public long? NullLongValue { get; set; }
        public ulong UlongValue { get; set; }
        public ulong? NullUlongValue { get; set; }
        public float FloatValue { get; set; }
        public float? NullFloatValue { get; set; }
        public double DoubleValue { get; set; }
        public double? NullDoubleValue { get; set; }
        public decimal DecimalValue { get; set; }
        public decimal? NullDecimalValue { get; set; }
        public bool BoolValue { get; set; }
        public bool? NullBoolValue { get; set; }
        public Guid GuidValue { get; set; }
        public Guid? NullGuidValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateTime? NullDateTimeValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
        public DateTimeOffset? NullDateTimeOffsetValue { get; set; }
        public DBNull DBNullValue { get; set; }
        public DBNull? NullDBNullValue { get; set; }

        public StructModel StructModel { get; set; }
    }

    public struct StructModel
    {
        public string StringValue { get; set; }

        public ValueModel ClassModel { get; set; }
    }
}
