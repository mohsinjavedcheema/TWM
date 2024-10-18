using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Classes;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class InstrumentTickersResponse : IResponse
    {
        [DataMember(Order = 1)]
        public List<InstrumentTicker> InstrumentTickers { get; set; }
    }
}
