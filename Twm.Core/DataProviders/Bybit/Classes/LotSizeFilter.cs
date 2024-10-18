
using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Bybit.Models.Classes
{


    [DataContract]    
    public class LotSizeFilter 
    {
        [DataMember(Order = 1, Name = "minNotionalValue")]
        public string MinNotionalValue { get; set; }

        [DataMember(Order = 2, Name = "maxOrderQty")]
        public string MaxOrderQty { get; set; }

        [DataMember(Order = 3, Name = "maxMktOrderQty")]
        public string MaxMktOrderQty { get; set; }

        [DataMember(Order = 4, Name = "minOrderQty")]
        public string MinOrderQty { get; set; }

        [DataMember(Order = 5, Name = "qtyStep")]
        public string QtyStep { get; set; }

    }


}
