using Twm.Core.DataProviders.Binance.Enums;
using Twm.Core.DataProviders.Binance.Models;

namespace Twm.Core.DataProviders.Binance
{
    public interface IBinanceInstrument
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
