using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Classes;
using Twm.Core.DataProviders.Bybit.Models.Response;

namespace Twm.Core.DataProviders.Bybit.Models.Classes
{
    [DataContract]
    public class OrdersResult
    {     
        [DataMember(Order = 1, Name = "category")]
        public string Category { get; set; }

        [DataMember(Order = 2, Name = "list")]
        public List<OrderResult> List { get; set; }
    }
}
