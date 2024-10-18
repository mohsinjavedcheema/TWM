using System;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Response following a call to the Get Klines Candlesticks endpoint
    /// </summary>
    [DataContract]    
    public class KlineCandleStick : IResponse
    {
        [DataMember(Order = 1)]
        public DateTime OpenTime { get; set; }

        [DataMember(Order = 2)]
        public decimal Open { get; set; }

        [DataMember(Order = 3)]
        public decimal High { get; set; }

        [DataMember(Order = 4)]
        public decimal Low { get; set; }

        [DataMember(Order = 5)]
        public decimal Close { get; set; }

        [DataMember(Order = 6)]
        public decimal Volume { get; set; }

        
        [DataMember(Order = 7)]
        public string Turnover { get; set; }

       
    }
}