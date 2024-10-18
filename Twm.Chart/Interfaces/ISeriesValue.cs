using System;

namespace Twm.Chart.Interfaces
{
    ///<summary>Represents the most widely used candlestick parameters.</summary>
    public interface ISeriesValue<T>
    {
        ///<summary>The time of the candlestick.</summary>
        ///<value>The time of the candlestick.</value>
        DateTime t { get; set; } // Момент времени включая дату и время

        T V { get; set; }

        
    }
}
