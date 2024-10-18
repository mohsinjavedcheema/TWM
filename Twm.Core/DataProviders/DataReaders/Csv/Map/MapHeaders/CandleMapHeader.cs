
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using Twm.Core.DataProviders.DataReaders.Csv.Map.Classes;

namespace Twm.Core.DataProviders.DataReaders.Csv.Map.MapHeaders
{
    public class CandleMapHeader : ClassMap<Candle>
    {
        public CandleMapHeader()
        {
            
            Map(p => p.Symbol).Index(1);
            Map(p => p.EventTime).Index(2).TypeConverterOption.Format("yyyyMMdd-HHmmss.fffzzz");
            Map(p => p.Time).Index(3).TypeConverterOption.Format("yyyyMMdd-HHmmsszzz");
            Map(p => p.Sequence).Index(4);
            Map(p => p.Count).Index(5).TypeConverter<NullableIntConverter>();
            Map(p => p.Open).Index(6).TypeConverter<NullableDoubleConverter>();
            Map(p => p.High).Index(7).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Low).Index(8).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Close).Index(9).TypeConverter<NullableDoubleConverter>();
            Map(p => p.Volume).Index(10).TypeConverter<NullableIntConverter>();
            Map(p => p.VWAP).Index(11).TypeConverter<NullableDoubleConverter>();
            Map(p => p.BidVolume).Index(12).TypeConverter<NullableIntConverter>();
            Map(p => p.AskVolume).Index(13).TypeConverter<NullableIntConverter>();
            Map(p => p.ImpVolatility).Index(14).TypeConverter<NullableDoubleConverter>();
            Map(p => p.OpenInterest).Index(15).TypeConverter<NullableLongConverter>();
        }


        public class NullableIntConverter : Int32Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }

        public class NullableDoubleConverter : DoubleConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }

        public class NullableLongConverter : Int64Converter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (text.ToLower() == "nan") return null;
                return base.ConvertFromString(text, row, memberMapData);
            }

        }
    }
}