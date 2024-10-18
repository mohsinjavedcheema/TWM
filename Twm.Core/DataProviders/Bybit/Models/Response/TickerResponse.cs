using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Bybit.Converter;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    /*[JsonConverter(typeof(TickersResponseConverter))]*/
    public class TickersResponse : IResponse
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public TickersList Result { get; set; }
    }
}
