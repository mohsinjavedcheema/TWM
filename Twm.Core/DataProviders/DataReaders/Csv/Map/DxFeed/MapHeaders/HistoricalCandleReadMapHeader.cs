using System;
using AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.Classes;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;

namespace AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.MapHeaders
{
    public class HistoricalCandleReadMapHeader : ClassMap<Candle>
    {
        public HistoricalCandleReadMapHeader()
        {
            Map(p => p.Time).Index(0).TypeConverterOption.Format("yyyyMMdd-HHmmss.fff");
            Map(p => p.Sequence).Index(1);
            Map(p => p.Count).Index(2).TypeConverter<NullableIntConverter>();
            Map(p => p.Open).Index(3).TypeConverter<NullableDoubleConverter>();
            Map(p => p.High).Index(4).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Low).Index(5).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Close).Index(6).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Volume).Index(7).TypeConverter<NullableDoubleConverter>();
            Map(p => p.VWAP).Index(8).TypeConverter<NullableDoubleConverter>();
            Map(p => p.BidVolume).Index(9).TypeConverter<NullableIntConverter>();
            Map(p => p.AskVolume).Index(10).TypeConverter<NullableIntConverter>();
            Map(p => p.ImpVolatility).Index(11).TypeConverter<NullableDoubleConverter>();
            Map(p => p.OpenInterest).Index(12).TypeConverter<NullableLongConverter>();
        }


        public class NullableIntConverter : Int32Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrEmpty(text) || text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }

        public class NullableDoubleConverter : DoubleConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrEmpty(text) || text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }

        public class NullableLongConverter : Int64Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.IsNullOrEmpty(text) || text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }
    }
}