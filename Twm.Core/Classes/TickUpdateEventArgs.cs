using System;
using Twm.Chart.Interfaces;


namespace Twm.Core.Classes
{
    public class TickUpdateEventArgs : EventArgs
    {
        public ICandle Candle { get; set; }

        public ICandle Tick { get; set; }

        public string  Symbol { get; set; }

        public TickUpdateEventArgs(ICandle candle, ICandle tick, string symbol)
        {
            Candle = candle;
            Tick = tick;
            Symbol = symbol;
        }
    }
}