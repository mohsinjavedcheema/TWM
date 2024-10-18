﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.WebSocket
{
    [DataContract]
    public class BybitAccountUpdateData: IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        #region Undefined API Result fields
        //TODO: Update when Bybit API updated

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "m")]
        public int M { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty(PropertyName = "t")]
        public int t { get; set; }

        [DataMember(Order = 5)]
        [JsonProperty(PropertyName = "b")]
        public int B { get; set; }

        [DataMember(Order = 6)]
        [JsonProperty(PropertyName = "s")]
        public int S { get; set; }

        [DataMember(Order = 7)]
        [JsonProperty(PropertyName = "T")]
        public bool T { get; set; }

        [DataMember(Order = 8)]
        [JsonProperty(PropertyName = "W")]
        public bool W { get; set; }

        [DataMember(Order = 9)]
        [JsonProperty(PropertyName = "D")]
        public bool D { get; set; }
        #endregion

        [DataMember(Order = 10)]
        [JsonProperty(PropertyName = "B")]
        public List<BalanceResponseData> Balances { get; set; }
    }
}