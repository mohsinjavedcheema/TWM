using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{


    [DataContract]    
    public class SymbolTicker 
    {
        [DataMember(Order = 1, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 2, Name = "lastPrice")]
        public string LastPrice { get; set; }

        [DataMember(Order = 3, Name = "indexPrice")]
        public string IndexPrice { get; set; }

        [DataMember(Order = 4, Name = "markPrice")]
        public string MarkPrice { get; set; }

        [DataMember(Order = 5, Name = "prevPrice24h")]
        public string PrevPrice24h { get; set; }


        [DataMember(Order = 6, Name = "price24hPcnt")]
        public string Price24hPcnt { get; set; }


        [DataMember(Order = 7, Name = "highPrice24h")]
        public string HighPrice24h { get; set; }
        
        [DataMember(Order = 8, Name = "lowPrice24h")]
        public string LowPrice24h { get; set; }

        [DataMember(Order = 9, Name = "prevPrice1h")]
        public string PrevPrice1h { get; set; }

        [DataMember(Order = 10, Name = "openInterest")]
        public string OpenInterest { get; set; }

        [DataMember(Order = 11, Name = "openInterestValue")]
        public string OpenInterestValue { get; set; }


        [DataMember(Order = 12, Name = "turnover24h")]
        public string Turnover24h { get; set; }


        [DataMember(Order = 13, Name = "volume24h")]
        public string Volume24h { get; set; }


        [DataMember(Order = 14, Name = "fundingRate")]
        public string FundingRate { get; set; }


        [DataMember(Order = 15, Name = "nextFundingTime")]
        public string NextFundingTime { get; set; }


        [DataMember(Order = 16, Name = "predictedDeliveryPrice")]
        public string PredictedDeliveryPrice { get; set; }


        [DataMember(Order = 17, Name = "basisRate")]
        public string BasisRate { get; set; }

        [DataMember(Order = 18, Name = "basis")]
        public string Basis { get; set; }

        [DataMember(Order = 19, Name = "deliveryFeeRate")]
        public string DeliveryFeeRate { get; set; }

        [DataMember(Order = 20, Name = "deliveryTime")]
        public string DeliveryTime { get; set; }

        [DataMember(Order = 21, Name = "ask1Size")]
        public string Ask1Size { get; set; }

        [DataMember(Order = 22, Name = "bid1Price")]
        public string Bid1Price { get; set; }

        [DataMember(Order = 23, Name = "ask1Price")]
        public string Ask1Price { get; set; }

        [DataMember(Order = 24, Name = "bid1Size")]
        public string Bid1Size { get; set; }






    }


}
