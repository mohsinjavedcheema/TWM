using Twm.Core.DataProviders.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class InstrumentsResponse : IResponse
    {
        [DataMember(Order = 1)]
        public List<BybitInstrument> Instruments { get; set; }
    }
}
