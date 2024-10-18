using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Futures
{
    [DataContract]
    public class BinanceAccountUpdateData: IWebSocketResponse
    {       
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

     
        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        [JsonProperty(PropertyName = "T")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime TransactionTime { get; set; }

        [JsonProperty(PropertyName = "a")]        
        public BinanceAccountData BinanceAccountData { get; set; }



    }
}