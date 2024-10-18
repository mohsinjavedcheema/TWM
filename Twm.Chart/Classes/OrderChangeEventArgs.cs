using System;

namespace Twm.Chart.Classes
{
    public class OrderChangeEventArgs :EventArgs
    {
        public string Guid { get; set; }

        public double? Quantity { get; set; }

        public double? Price { get; set; }


        public OrderChangeEventArgs(string guid, double? quantity, double? price)
        {
            Guid = guid;
            Quantity = quantity;
            Price = price;
        }

    }
}