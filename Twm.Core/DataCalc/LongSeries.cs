using Twm.Core.Enums;

namespace Twm.Core.DataCalc
{
    public class LongSeries:Series<long>
    {
        

        public LongSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty) : base(dataCalcContext, candleProperty)
        {
            ExtraSeriesIndex = -1;
            IsExtraSeries = false;
            
        }

        public LongSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty, int extraSeriesIndex) : base(dataCalcContext, candleProperty)
        {
            ExtraSeriesIndex = extraSeriesIndex;
            IsExtraSeries = true;
            Reset();
        }

        public override long this[int key]
        {
            get
            {
                if (!IsExtraSeries)
                {
                   /* if (_candleProperty == CandleProperty.Volume)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].V;
                    }*/

                }
                else
                {
                    /*var candleIndex = GetExtraSeriesCandleIndex(key);
                    if (candleIndex == -1)
                        return 0;

                    if (_candleProperty == CandleProperty.Volume)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].V;
                    }*/

                }

                return base[key];

            }
            set { base[key] = value; }
        }


      
    }
}