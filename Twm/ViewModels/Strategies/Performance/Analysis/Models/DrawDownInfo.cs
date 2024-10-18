using System;
using OxyPlot;
using OxyPlot.Axes;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Models
{
    public class DrawDownInfo: IDataPointProvider, ICloneable
    {
        public string ExitDateString
        {
            get { return ExitDate.ToString("dd.MM.yyyy HH:mm:ss"); }
        }

        public DateTime ExitDate { get; set; }
        public int TradeNumber { get; set; }
        public double CurrentDrawDown { get; set; }

        public string StrategyId { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(DateTimeAxis.ToDouble(ExitDate), CurrentDrawDown);
        }
    }
}