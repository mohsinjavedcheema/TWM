using System;


namespace Twm.Core.DataProviders.Binance.Models
{
    [Serializable]
    public class InstrumentTicker 
    {        
        public string Symbol { get; set; }

        public double LastPrice { get; set; }

        public double Volume { get; set; }
       
    }
}
