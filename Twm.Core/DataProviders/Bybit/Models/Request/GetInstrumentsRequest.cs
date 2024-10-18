using System.Runtime.Serialization;
using Twm.Core.DataProviders.Interfaces;


namespace Twm.Core.DataProviders.Bybit.Models.Request
{
    [DataContract]
    public class GetInstrumentsRequest : IRequest
    {
        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }


    }
}
