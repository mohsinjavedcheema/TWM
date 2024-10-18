using Twm.Core.DataProviders.Binance.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Binance.Converter;

namespace Twm.Core.DataProviders.Binance.Models.Request
{
    /// <summary>
    /// Request object used to modify a Binance order
    /// </summary>
    [DataContract]
    public class ModifyOrderRequest: IRequest
    {

        [DataMember(Order = 1)]
        public long OrderId { get; set; }

        [DataMember(Order = 2)]
        public string OrigClientOrderId { get; set; }


        [DataMember(Order = 3)]
        public string Symbol { get; set; }

        [DataMember(Order = 4)]
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get; set; }        
        

        [DataMember(Order = 5)]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal Quantity { get; set; }

        [DataMember(Order = 6)]
        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? Price { get; set; }

        

        

        

        
    }
}
