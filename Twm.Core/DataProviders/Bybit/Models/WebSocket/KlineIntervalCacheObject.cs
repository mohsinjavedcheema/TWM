using System.Collections.Generic;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    public class KlineIntervalCacheObject
    {
        public Dictionary<long, KlineCandleStick> TimeKlineDictionary { get; set; }
    }
}