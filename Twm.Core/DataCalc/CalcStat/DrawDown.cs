using System;
using System.Collections.Generic;

namespace Twm.Core.DataCalc.CalcStat
{
    public class DrawDown
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<double> Values { get; set; }
        public double PeriodMaxDrawDown { get; set; }
        public double DrawDownPeriodDays { get; set; }
        public double StartEquity { get; set; }

        public int TotalDays { get; set; }
    }
}