using System.Collections.Generic;
using Twm.Chart.Controls;
using Twm.Chart.Interfaces;

namespace Twm.Chart.Classes
{
    public class ValuesExtremums
    {

        public double CandleValueHigh;

        public double CandleValueLow;


        public Dictionary<PaneControl, double> ValuesHigh;

        public Dictionary<PaneControl, double> ValuesLow;

        public bool IsValuesCalculated;

        
        public ValuesExtremums(double valueHigh, double valueLow)
        {
            CandleValueHigh = valueHigh;
            CandleValueLow = valueLow;
            ValuesHigh = new Dictionary<PaneControl, double>();
            ValuesLow = new Dictionary<PaneControl, double>();
            IsValuesCalculated = false;
        }


        public double GetValuesHigh(PaneControl paneControl)
        {
            if (ValuesHigh.TryGetValue(paneControl, out var value))
            {
                return value;
            }

            return 0;
        }

        public double GetValuesLow(PaneControl paneControl)
        {
            if (ValuesLow.TryGetValue(paneControl, out var value))
            {
                return value;
            }

            return 0;
        }


#pragma warning  disable CS1591
        public override bool Equals(object obj) { return false; }
    }
}