using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Common.OrderBooks;

namespace Twm.Core.DataProviders.Bybit.Models.Classes
{
    [DataContract]
    public class PriceLevel
    {
        public MarketDataType MarketDataType { get; set; }

        [DataMember(Name = "price")]
        public double Price { get; set; }

        [DataMember(Name = "volume")]
        public double Volume { get; set; }


        [DataMember(Name = "cost")]
        public double Cost { get; set; }


        public PriceLevel(MarketDataType marketDataType, double price, double volume, double cost)
        {
            MarketDataType = marketDataType;
            Price = price;
            Volume = volume;
            Cost = cost;
        }
    }
}
