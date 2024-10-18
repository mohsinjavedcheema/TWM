using System;


namespace Twm.Core.DataProviders.Bybit.Models
{
    [Serializable]
    public class InstrumentTicker 
    {        
        public string Symbol { get; set; }

        public double LastPrice { get; set; }

        public double Volume { get; set; }
       
    }
}
