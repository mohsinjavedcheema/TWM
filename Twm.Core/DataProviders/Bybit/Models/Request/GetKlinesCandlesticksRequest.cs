using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    /// <summary>
    /// Request object used to get Klines/Candlesticks
    /// </summary>
    [DataContract]
    public class GetKlinesCandlesticksRequest : IRequest
    {


        [DataMember(Order = 1)]
        public string Category { get; set; }

        [DataMember(Order = 2)]
        public string Symbol { get; set; }

        [DataMember(Order = 3)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KlineInterval Interval { get; set; }

        [DataMember(Order = 4)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime? Start { get; set; }

        [DataMember(Order = 5)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime? End { get; set; }

        [DataMember(Order = 6)]
        public int? Limit { get; set; }

        

        public bool IsTestMode { get; set; }

    }
}
