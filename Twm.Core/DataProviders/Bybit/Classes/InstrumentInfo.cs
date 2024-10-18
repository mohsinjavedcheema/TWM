using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Bybit.Models.Classes
{


    [DataContract]    
    public class InstrumentInfo 
    {
        [DataMember(Order = 1, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 2, Name = "contractType")]
        public string ContractType { get; set; }

        [DataMember(Order = 3, Name = "status")]
        public string Status { get; set; }


        [DataMember(Order = 4, Name = "baseCoin")]
        public string BaseCoin { get; set; }        


        [DataMember(Order = 5, Name = "quoteCoin")]
        public string QuoteCoin { get; set; }


        [DataMember(Order = 6, Name = "launchTime")]
        public string LaunchTime { get; set; }



        [DataMember(Order = 7, Name = "deliveryTime")]
        public string DeliveryTime { get; set; }



        



        [DataMember(Order = 8, Name = "deliveryFeeRate")]
        public string DeliveryFeeRate { get; set; }



        [DataMember(Order = 9, Name = "priceScale")]
        public string PriceScale { get; set; }


        [DataMember(Order = 11, Name = "priceFilter")]
        public PriceFilter PriceFilter { get; set; }

        [DataMember(Order = 12, Name = "lotSizeFilter")]
        public LotSizeFilter LotSizeFilter { get; set; }


    }


}
