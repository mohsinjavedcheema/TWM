using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.DataProviders.Bybit.Models.Classes;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Response object received when querying a Bybit open orders
    /// </summary>
    [DataContract]
    public class OrdersResponse: IResponse
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public OrdersResult Result { get; set; }
    }
}