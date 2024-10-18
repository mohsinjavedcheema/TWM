using Twm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.DataProviders.Bybit.Models.Response.Custom
{
    public class HistoricalCandle : IHistoricalCandle
    {
        public string Symbol { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime Time { get; set; }

        public DateTime CloseTime { get; set; }
        public string Sequence { get; set; }
        public int? Count { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? Volume { get; set; }
        public double? VWAP { get; set; }
        public int? BidVolume { get; set; }
        public int? AskVolume { get; set; }
        public double? ImpVolatility { get; set; }
        public long? OpenInterest { get; set; }

        public bool IsClosed { get; set; }
    }
}
