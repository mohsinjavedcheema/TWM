namespace Twm.Chart.Classes
{
    internal struct CandleDrawingParameters
    {
        public double Width;
        public double Gap;
        public CandleDrawingParameters(double width, double gapBetweenCandles)
        {
            Width = width;
            Gap = gapBetweenCandles;
        }
    }
}