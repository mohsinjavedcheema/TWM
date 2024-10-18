using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]
    public class TickersList
    {     
        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }

        [DataMember(Order = 2, Name = "list")]
        public List<SymbolTicker> List { get; set; }
    }
}
