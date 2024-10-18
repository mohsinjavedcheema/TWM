using System;

namespace Twm.Chart.DrawObjects
{
    public class OrderDraw:ICloneable
    {
        public string Guid { get; set; }

        public string OrderType { get; set; }

        public string OrderAction { get; set; }

        public double StopPrice { get; set; }

        public double LimitPrice { get; set; }

        public double Qnt { get; set; }


        public double Price
        {
            get
            {
                if (OrderType.ToUpper() == "LIMIT")
                    return LimitPrice;
                if (OrderType.ToUpper() == "STOPMARKET")
                    return StopPrice;
                if (OrderType.ToUpper() == "STOPLIMIT")
                    return StopPrice;
                return 0;
            }
        }

        public bool IsStopOrder
        {
            get
            {
                var type = OrderType.ToUpper();

                return type == "STOPMARKET" || type == "STOPLIMIT";
            }
        }

        public bool IsLimitOrder
        {
            get
            {
                return OrderType.ToUpper() == "LIMIT";
            }
        }

        public string Name
        {
            get
            {
                if (OrderAction.ToUpper() == "SELL" || OrderAction.ToUpper() == "SELLSHORT") 
                {
                    if (IsLimitOrder)
                    {
                        return "SELL LMT";
                    }
                    if (IsStopOrder)
                    {
                        var type = OrderType.ToUpper();
                        if (type == "STOPMARKET")
                            return "SELL STPM";
                        return "SELL STPL";
                    }
                }
                else if (OrderAction.ToUpper() == "BUY" || OrderAction.ToUpper() == "BUYTOCOVER")
                {
                    if (IsLimitOrder)
                    {
                        return "BUY LMT";
                    }
                    if (IsStopOrder)
                    {
                        var type = OrderType.ToUpper();
                        if (type == "STOPMARKET")
                            return "BUY STPM";

                        return "BUY STPL";
                    }
                }

                return "";
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}