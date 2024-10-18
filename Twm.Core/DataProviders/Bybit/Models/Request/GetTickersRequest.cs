using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;


namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    [DataContract]
    public class GetTickersRequest : IRequest
    {
        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }

        [DataMember(Order = 2, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 3, Name = "basecoin")]
        public string BaseCoin { get; set; }

        [DataMember(Order = 4, Name = "expDate")]
        public string ExpDate { get; set; }




    }
}
