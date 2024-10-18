using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;

namespace Twm.Chart.Classes
{

    public sealed class Plot:ICloneable
    {
        [Browsable(false)] 
        //Script object that created this Plot
        public object Owner { get; private set; }


        [Browsable(false)]
        public bool IsAutoscale { get; set; }


        [Browsable(false)] 
        public string ParentName { get; private set; }

        [Browsable(false)]
        public ISeries<double> DataSource { get; set; }

        public PlotLineType LineType { get; set; }
        public PlotChartType ChartType { get; set; }

        public IDictionary<int, Brush> PlotColors { get; set; } = new Dictionary<Int32, Brush>();

        public Color Color { get; set; }

        public double Thickness { get; set; }

        public string Name { get; set; }

        public bool IsDataSourceEmpty
        {
            get
            {
                return DataSource == null || DataSource.Count == 0;
            }
        }



        public Plot(Color? color = null, String name = "",
            double thickness = 2.0d, 
            PlotLineType lineType = PlotLineType.Solid, 
            PlotChartType chartType = PlotChartType.Linear )
        {
            Name = name;
            Color = color ?? Colors.Black;
            LineType = lineType;
            Thickness = thickness;
            ChartType = chartType;
        }
        
        public Plot():this(Colors.Black)
        {
            
        }
        
        public void SetOwner(object owner)
        {
            Owner = owner;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Name = name;
            }

            if (string.IsNullOrEmpty(ParentName))
            {
                ParentName = name;
            }
        }

       
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return "Plot";
            }

            return Name;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
