using System;
using OxyPlot;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Models
{
    public class TradeInfo: IDataPointProvider, ICloneable
    {
        public double Profit { get; set; }
        public DateTime ExitDate { get; set; }

        public string ExitDateString
        {
            get { return ExitDate.ToString("dd.MM.yyyy HH:mm:ss"); }
        }
        public int TradeNumber { get; set; }

        public int Index { get; set; }
        public double CumProfit { get; set; }
        public double CurrentDrawDown { get; set; }

        public string StrategyId { get; set; }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(Index, CumProfit);
        }
    }
}