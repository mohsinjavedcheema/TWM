using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.Classes
{
    [DataContract]
    public class InstrumentInfoList
    {

        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }

        [DataMember(Order = 2, Name = "list")]
        public List<InstrumentInfo> List { get; set; }
    }
}
