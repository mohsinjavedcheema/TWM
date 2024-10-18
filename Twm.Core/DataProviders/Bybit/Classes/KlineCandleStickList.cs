using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    
    public class KlineCandleStickList
    {
        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }

        [DataMember(Order = 2, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 3, Name = "list")]
        [JsonConverter(typeof(KlineCandleSticksConverter))]
        public List<KlineCandleStick> List { get; set; }
    }
}
