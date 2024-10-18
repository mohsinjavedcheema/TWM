using Twm.Core.Enums;

namespace Twm.Core.DataCalc
{
    public class DoubleSeries:Series<double>
    {
        

        public DoubleSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty) : base(dataCalcContext, candleProperty)
        {
            ExtraSeriesIndex = -1;
            IsExtraSeries = false;
        }

        public DoubleSeries(DataCalcContext dataCalcContext, CandleProperty candleProperty, int extraSeriesIndex) : base(dataCalcContext, candleProperty, extraSeriesIndex)
        {
            ExtraSeriesIndex = extraSeriesIndex;
            IsExtraSeries = true;
        }

        public override double this[int key]
        {
            get
            {
                if (!IsExtraSeries)
                {
                    if (_candleProperty == CandleProperty.Open)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].O;
                    }

                    if (_candleProperty == CandleProperty.High)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].H;
                    }

                    if (_candleProperty == CandleProperty.Low)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].L;
                    }

                    if (_candleProperty == CandleProperty.Close)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].C;
                    }

                    if (_candleProperty == CandleProperty.Volume)
                    {
                        return DataCalcContext.Candles[DataCalcContext.CurrentBar - key - 1].V;
                    }
                }
                else
                {
                    var candleIndex = GetExtraSeriesCandleIndex(key);
                    if (candleIndex == -1)
                        return 0;

                    if (_candleProperty == CandleProperty.Open)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].O;
                    }

                    if (_candleProperty == CandleProperty.High)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].H;
                    }

                    if (_candleProperty == CandleProperty.Low)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].L;
                    }

                    if (_candleProperty == CandleProperty.Close)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].C;
                    }

                    if (_candleProperty == CandleProperty.Volume)
                    {
                        return DataCalcContext.ExtraDataSeries[ExtraSeriesIndex].Candles[candleIndex].V;
                    }


                }

                return base[key];

            }
            set { base[key] = value; }
        }


      
    }
}