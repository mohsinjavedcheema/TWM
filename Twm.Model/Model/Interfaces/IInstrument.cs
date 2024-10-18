using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Twm.Model.Model.Interfaces
{
    public interface IInstrument
    {
        string DpId { get; set; }

        string Type { get; set; }

        string Symbol { get; set; }

        string Description { get; set; }

        string Base { get; set; }//

        
        string Quote { get; set; }//

        double? Multiplier { get; set; }

        double? MinLotSize { get; set; }//       

        double? Notional { get; set; }//       

        string PriceIncrements { get; set; }

        string TradingHours { get; set; }

    }
}