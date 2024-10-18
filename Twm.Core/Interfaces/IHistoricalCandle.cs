using System;

namespace Twm.Core.Interfaces
{
    public interface IHistoricalCandle
    {
        string Symbol { get; set; }
        DateTime EventTime { get; set; }
        DateTime Time { get; set; }
        DateTime CloseTime { get; set; }
        string Sequence { get; set; }
        int? Count { get; set; }
        double? Open { get; set; }
        double? High { get; set; }
        double? Low { get; set; }
        double? Close { get; set; }
        double? Volume { get; set; }
        double? VWAP { get; set; }
        int? BidVolume { get; set; }
        int? AskVolume { get; set; }
        double? ImpVolatility { get; set; }
        long? OpenInterest { get; set; }

        bool IsClosed { get; set; }
    }
}