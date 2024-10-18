using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Twm.Core.DataProviders.Bybit.Enums;

namespace Twm.Core.DataProviders.Interfaces
{
    public interface IPriceLevel
    {

        double Price { get; set; }
        double Volume { get; set; }

        double Cost { get; set; }



    }
}
