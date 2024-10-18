using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Twm.Core.DataCalc.Performance;

namespace Twm.Core.DataCalc.MoneyManagement
{
    public abstract class MoneyManagement : DataCalcObject, ICloneable
    {

        [Browsable(true)]
        [Display(Name = "Starting capital", Order = 0)]
        public double StartingCapital { get; set; }


        [Browsable(false)]
        public SystemPerformance SystemPerformance { get; set; }

        public abstract int GetQuantity(double price, double mult);

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return "";
        }
    }
}