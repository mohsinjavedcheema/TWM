using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Twm.Chart.Interfaces;
using Twm.Core.Controllers;


namespace Twm.Core.DataCalc
{
    public abstract class IndicatorBase : ScriptBase, ISeries<double>, ICloneable
    {
        [Browsable(false)]
        public string DisplayName
        {
            get
            {
                var values = string.Join(", ", TwmPropertyValues.Values.Select(x => x == null ? "": x.ToString()));
                if (!string.IsNullOrEmpty(values))
                    values = ", " + values;
                return $"{GetType().Name}({Input}{values})";
            }
        }


        [Browsable(false)]
        public int Count
        {
            //get { return Values.Count; }
            get { return _values[0].Count; }
        }

        public double GetValueAt(int index)
        {
            //return Input[index];
            return _values[0].GetValueAt(index);
        }

        public double[] GetRange(int startIndex, int count)
        {
            //return Input.GetRange(startIndex, count);
            return _values[0].GetRange(startIndex, count);
        }


        [Browsable(false)]
        public double this[int key]
        {
            get { return _values[0][key]; }
            set { _values[0][key] = value; }
        }

        [Browsable(false)] public List<ISeriesValue<double>> Data { get; set; }


        protected IndicatorBase()
        {
            _values = new List<ISeries<double>>();
            Options = new ScriptOptions();
        }


        /// <summary>
        /// Executed before indicator set state Configured
        /// </summary>
        public override void BeforeConfigured()
        {
            ApplyOptions();            
        }


        /// <summary>
        /// Executed before indicator set state Configured
        /// </summary>
        public override void AfterConfigured()
        {
            ApplyOptions();
        }


        private void ApplyOptions()
        {
            if (!Options.ShowPanes)
            {
                if (Chart != null)
                {
                    RemovePanes();
                }
            }

            if (!Options.ShowPlots)
            {
                RemovePlots();
            }
        }

        /// <summary>
        /// Clear all debug messages
        /// </summary>
        public override void ClearDebug()
        {
            if (!IsOptimization)
                DebugController.Clear();
        }


        /// <summary>
        /// Print message to debug
        /// </summary>
        /// <param name="message"></param>
        public override void Print(object message)
        {
            if (!IsOptimization)
                DebugController.Print(message.ToString());
        }


        public DataCalcContext GetDataCalcContext()
        {
            return DataCalcContext;
        }


        public bool EqualsInput(ISeries<double> series)
        {
            return Input == series;
            if (Input == null && series == null)
                return true;

            if (Input != null && series == null)
                return false;

            if (Input == null && series != null)
                return false;

            if (Input.Count != series.Count)
                return false;

            for (int i = 0; i < Input.Count; i++)
            {
                if (Input.GetValueAt(i) != series.GetValueAt(i))
                    return false;
            }

            return true;
        }


        public object Clone()
        {
            return MemberwiseClone();
        }


        public override void OnAfterBarUpdate()
        {
            _values.ForEach(x => x.SetValid(0));
            LastBarIndex = CurrentBar;
        }

        public bool IsValidPoint(int index)
        {
            return Input.IsValidPoint(index);
        }

        public void SetValid(int index)
        {
            Input.SetValid(index);
        }


        public override string ToString()
        {
            return DisplayName;
        }


        public override void Reset()
        {
            base.Reset();
            Chart?.Reset();
        }
    }
}