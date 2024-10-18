using System;
using System.Collections.Generic;
using Twm.Core.Enums;
using Twm.Core.Market;

namespace Twm.Core.Classes
{
    public class OrderChangeEventArgs : EventArgs
    {
        public List<Order> Orders { get; set; }

        public OrderChangeAction ChangeAction { get; set; }

        public OrderChangeEventArgs(List<Order> orders, OrderChangeAction changeAction)
        {
            Orders = orders;
            ChangeAction = changeAction;
        }
    }
}