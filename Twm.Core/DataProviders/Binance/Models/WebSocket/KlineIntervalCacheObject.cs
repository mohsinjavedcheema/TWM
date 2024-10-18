using System.Collections.Generic;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    public class KlineIntervalCacheObject
    {
        public Dictionary<long, KlineCandleStick> TimeKlineDictionary { get; set; }
    }
}