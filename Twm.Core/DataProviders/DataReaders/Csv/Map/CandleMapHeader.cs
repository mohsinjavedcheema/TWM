using Twm.Chart.Classes;
using CsvHelper.Configuration;

namespace Twm.Core.DataProviders.DataReaders.Csv.Map
{
    public class CandleMapHeader : ClassMap<Candle>
    {
        public CandleMapHeader()
        {
            Map(p => p.t).Index(0).TypeConverterOption.Format("yyyyMMdd HHmmss");
            Map(p => p.O).Index(1);
            Map(p => p.H).Index(2);
            Map(p => p.L).Index(3);
            Map(p => p.C).Index(4);
            Map(p => p.V).Index(5);
        }
    }
}