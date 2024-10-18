﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Binance.Converter;
using Twm.Core.DataProviders.Binance.Models.Response;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    [DataContract]
    public class BinancePartialDepthData : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "stream")]
        public string Stream { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "data")]
        public BinancePartialData Data { get; set; }


        public string EventType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime EventTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
