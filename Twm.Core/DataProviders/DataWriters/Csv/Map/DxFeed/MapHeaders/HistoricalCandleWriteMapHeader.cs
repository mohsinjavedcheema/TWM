using Twm.Core.Interfaces;
using CsvHelper.Configuration;

namespace Twm.Core.DataProviders.DataWriters.Csv.Map.DxFeed.MapHeaders
{
    public class HistoricalCandleWriteMapHeader : ClassMap<IHistoricalCandle>
    {
        public HistoricalCandleWriteMapHeader()
        {
            
            Map(p => p.Time).Index(0).Name("Time").TypeConverterOption.Format("yyyyMMdd-HHmmss.fff");
            Map(p => p.Sequence).Index(1).Name("Sequence");
            Map(p => p.Count).Index(2).Name("Count");
            Map(p => p.Open).Index(3).Name("Open");
            Map(p => p.High).Index(4).Name("High");
            Map(p => p.Low).Index(5).Name("Low");
            Map(p => p.Close).Index(6).Name("Close");
            Map(p => p.Volume).Index(7).Name("Volume");
            Map(p => p.VWAP).Index(8).Name("VWAP");
            Map(p => p.BidVolume).Index(9).Name("BidVolume");
            Map(p => p.AskVolume).Index(10).Name("AskVolume");
            Map(p => p.ImpVolatility).Index(11).Name("ImpVolatility");
            Map(p => p.OpenInterest).Index(12).Name("OpenInterest");
        }


    }
}