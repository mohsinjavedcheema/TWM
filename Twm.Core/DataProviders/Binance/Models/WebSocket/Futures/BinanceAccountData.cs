using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class BinanceAccountData
    {       
        [JsonProperty(PropertyName = "m")]
        public string ReasonType { get; set; }
            
        [JsonProperty(PropertyName = "B")]        
        public List<BalanceData> Balances { get; set; }


        [JsonProperty(PropertyName = "P")]
        public List<PositionData> Positions { get; set; }


    }
}