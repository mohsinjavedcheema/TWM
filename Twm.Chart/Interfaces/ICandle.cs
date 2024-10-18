using System;

namespace Twm.Chart.Interfaces
{
    ///<summary>Represents the most widely used candlestick parameters.</summary>
    public interface ICandle
    {
        ///<summary>The time of the candlestick.</summary>
        ///<value>The time of the candlestick.</value>
        DateTime t { get; set; } // Момент времени включая дату и время


        ///<summary>The time of the candlestick.</summary>
        ///<value>The time of the candlestick.</value>
        DateTime ct { get; set; } // Момент времени включая дату и время


        ///<summary>The Open of the candlestick (opening price).</summary>
        ///<value>The Open of the candlestick (opening price).</value>
        double O { get; set; }

        ///<summary>The High of the candlestick (price maximum).</summary>
        ///<value>The High of the candlestick (price maximum).</value>
        double H { get; set; }

        ///<summary>The Low of the candlestick (price minimum).</summary>
        ///<value>The Low of the candlestick (price minimum).</value>
        double L { get; set; }

        ///<summary>The Close of the candlestick (closing price).</summary>
        ///<value>The Close of the candlestick (closing price).</value>
        double C { get; set; }

        ///<summary>The Volume of the candlestick.</summary>
        ///<value>The Volume of the candlestick.</value>
        double V { get; set; }

        bool IsClosed { get; set; }

        bool IsFirstTickOfBar { get; set; }

        bool IsAggVolume { get; set; }
    }
}
