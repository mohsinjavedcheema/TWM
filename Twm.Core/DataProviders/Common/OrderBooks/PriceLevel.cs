using System;
using System.Runtime.Serialization;


namespace Twm.Core.DataProviders.Common.OrderBooks
{
    [DataContract]
    public class PriceLevel
    {
        public double Price { get; set; }


        public double Size { get; set; }


        public DateTime Time { get; set; }


        public PriceLevel(double price, double size, DateTime time)
        {
            Price = price;
            Size = size;
            Time = time;
        }
    }
}
