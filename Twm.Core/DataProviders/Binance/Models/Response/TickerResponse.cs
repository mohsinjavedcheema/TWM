using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Twm.Core.DataProviders.Binance.Converter;


namespace Twm.Core.DataProviders.Binance.Models.Response
{
    [DataContract]
    [JsonConverter(typeof(TickersResponseConverter))]
    public class TickersResponse : IResponse
    {
        [DataMember]
        public List<TickerInfo> InstrumentTickers { get; set; }
    }
}
