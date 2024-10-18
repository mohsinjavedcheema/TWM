using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models;

namespace Twm.Core.DataProviders.Bybit
{
    public interface IBybitInstrument
    {
        MarketType Type { get; set; }

        string Symbol { get; set; }

        string Status { get; set; }

        string BaseAsset { get; set; }

        int BaseAssetPrecision { get; set; }

        string QuoteAsset { get; set; }

        int QuotePrecision { get; set; }
    

        Filter[] Filters { get; set; }

        
    }
}
