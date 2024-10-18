using System.Collections.Generic;
using Twm.Chart.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;

namespace Twm.Core.DataCalc.DataSeries
{
    public class ExtraDataSeries : DataSeriesParams
    {
        public override DataSeriesType DataSeriesType
        {
            get { return base.DataSeriesType; }
            set
            {
                if (value != base.DataSeriesType)
                {
                    base.DataSeriesType = value;
                    DataSeriesLength = GetLengthInMinutes();
                }
            }
        }
  
        public override int DataSeriesValue
        {
            get { return base.DataSeriesValue; }
            set
            {
                if (value != base.DataSeriesValue)
                {
                    base.DataSeriesValue = value;
                    DataSeriesLength = GetLengthInMinutes();
                }
            }
        }
        

        public List<ICandle> Candles { get; set; }

        public int LastIndex { get; set; }


        public ExtraDataSeries()
        {
            LastIndex = -1;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}