using System;
using Twm.Chart.Interfaces;

namespace Twm.Core.DataCalc
{
    public class SeriesValue<T> :ISeriesValue<T>
    {
        public DateTime t { get; set; }
        public T V { get; set; }
        
        public SeriesValue(DateTime t, T V)
        {
            this.t = t;
            this.V = V;

        }
    }
}