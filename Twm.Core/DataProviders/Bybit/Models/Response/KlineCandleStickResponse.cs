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
    public class KlineCandleStickResponse : IResponse
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public KlineCandleStickList Result { get; set; }
    }
}