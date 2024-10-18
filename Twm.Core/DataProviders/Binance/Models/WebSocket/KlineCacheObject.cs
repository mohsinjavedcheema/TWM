using System.Collections.Generic;
using Twm.Core.DataProviders.Binance.Enums;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    public class KlineCacheObject
    {
        public Dictionary<KlineInterval, KlineIntervalCacheObject> KlineInterDictionary { get; set; }   
    }
}