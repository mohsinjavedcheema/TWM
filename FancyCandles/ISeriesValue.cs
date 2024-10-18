using System;

namespace FancyCandles
{
    ///<summary>Represents the most widely used candlestick parameters.</summary>
    public interface ISeriesValue
    {
        ///<summary>The time of the candlestick.</summary>
        ///<value>The time of the candlestick.</value>
        DateTime t { get; } // Момент времени включая дату и время

        
        double V { get;}

        
    }
}
