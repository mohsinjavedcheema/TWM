
using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Bybit.Models.Classes
{


    [DataContract]    
    public class PriceFilter 
    {
        [DataMember(Order = 1, Name = "minPrice")]
        public string MinPrice { get; set; }

        [DataMember(Order = 2, Name = "maxPrice")]
        public string MaxPrice { get; set; }

        [DataMember(Order = 3, Name = "tickSize")]
        public string TickSize { get; set; }

    }


}
