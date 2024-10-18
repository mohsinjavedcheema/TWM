using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.DataProviders.Common.Response
{
    [DataContract]
    public class HistoricalDataResponse : IResponse
    {
        [DataMember(Order = 1)]
        public List<IHistoricalCandle> Candles { get; set; }
    }
}
