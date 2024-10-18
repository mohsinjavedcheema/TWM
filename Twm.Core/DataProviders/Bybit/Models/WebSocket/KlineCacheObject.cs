using System.Collections.Generic;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    public class KlineCacheObject
    {
        public Dictionary<KlineInterval, KlineIntervalCacheObject> KlineInterDictionary { get; set; }   
    }
}