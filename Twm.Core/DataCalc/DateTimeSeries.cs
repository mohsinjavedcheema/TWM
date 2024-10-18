using System;
using Twm.Core.Enums;

namespace Twm.Core.DataCalc
{
    public class DateTimeSeries : Series<DateTime>
    {
        public DateTimeSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty) : base(dataCalcContext,
            candleProperty)
        {
            ExtraSeriesIndex = -1;
            IsExtraSeries = false;
            Reset();
        }

        public DateTimeSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty, int extraSeriesIndex) :
            base(dataCalcContext, candleProperty)
        {
            ExtraSeriesIndex = extraSeriesIndex;
            IsExtraSeries = true;
            Reset();
        }

        public override DateTime this[int key]
        {
            get
            {
                if (!IsExtraSeries)
                {
                    if (_candleProperty == CandleProperty.DateTime)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].t;
                    }
                }
                else
                {

                    var candleIndex = GetExtraSeriesCandleIndex(key);
                    if (candleIndex == -1)
                         throw new IndexOutOfRangeException("Extra data series index: " + candleIndex);


                    if (_candleProperty == CandleProperty.DateTime)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].t;
                    }
                }


                return base[key];
            }
            set { base[key] = value; }
        }

       
    }
}