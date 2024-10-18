using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Twm.Core.Enums;

namespace Twm.Core.DataCalc
{
    public class BarSeries
    {
        protected DataCalcContext DataCalcContext { get; set; }

        public BarSeries(DataCalcContext dataCalcContext) 
        {
            DataCalcContext = dataCalcContext;
        }

        public virtual int this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return DataCalcContext.CurrentBar;
                }
                else
                {
                    return GetExtraSeriesCandleIndex(index);
                }

            }
        }


        protected int GetExtraSeriesCandleIndex(int index)
        {
            var seriesIndex = index - 1;

            var curTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].ct;
            var candleIndex = DataCalcContext.ExtraDataSeries[seriesIndex].LastIndex;

            var i = candleIndex;
            if (i == -1)
                i = 0;

            bool isExist = false;
            var length = DataCalcContext.ExtraDataSeries[seriesIndex].Candles.Count;

            var isLess = true;
            while (DataCalcContext.ExtraDataSeries[seriesIndex].Candles[i].ct <= curTime)
            {

                if (DataCalcContext.ExtraDataSeries[seriesIndex].Candles[i].ct.ToUniversalTime().Date == curTime.ToUniversalTime().Date)
                {
                    isLess = false;
                }

                i++;
                isExist = true;

                if (i >= length)
                    break;
            }


            if (isExist)
            {
                candleIndex = i - 1;
                if (candleIndex < 0)
                {
                    return 0;
                }

                DataCalcContext.ExtraDataSeries[seriesIndex].LastIndex = candleIndex;
            }
            else
            {
                return 0;
            }

            if (DataCalcContext.ExtraDataSeries[seriesIndex].DataSeriesType == DataSeriesType.Day ||
                DataCalcContext.ExtraDataSeries[seriesIndex].DataSeriesType == DataSeriesType.Month)
            {
                if (!isLess)
                {
                    if (candleIndex - 1 >= 0)
                    {
                        candleIndex--;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            return candleIndex + 1;
        }
    }
}