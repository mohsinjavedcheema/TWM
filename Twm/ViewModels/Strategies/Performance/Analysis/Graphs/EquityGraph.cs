using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Twm.ViewModels.Strategies.Performance.Analysis.Models;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;


namespace Twm.ViewModels.Strategies.Performance.Analysis.Graphs
{
    public class EquityGraph : BaseGraphViewModel
    {
        public EquityGraph()
        {
            XTitle = "TradeNo";
            YTitle = "Profit";
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
        }


        public override void CreatePlotModel()
        {
            var model = new ViewResolvingPlotModel { IsLegendVisible = true, LegendPosition = LegendPosition.RightMiddle, LegendPlacement = LegendPlacement.Outside};

            model.Axes.Add(CreateTradesAxis());
            model.Axes.Add(CreateValueAxis());

            TradesAxis.Title = XTitle;
            ValueAxis.Title = YTitle;
            ValueAxis.LabelFormatter = YFormatter;

            if (Data != null && Data.Any())
            {
                SetMaxMin(TradesAxis, 0,Data.Count);
                SetMaxMin(ValueAxis, Data.Min(x => x.CumProfit), Data.Max(x => x.CumProfit));
            }

            Model = model;
        }

        public override async Task<bool> LoadData()
        {
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Green,
                ItemsSource = Data.Select((x, index) => new TradeInfo()
                    {
                        Index = index,
                        CumProfit = Math.Round(x.CumProfit, 2),
                        ExitDate = x.ExitTime,
                        TradeNumber = (int)x.TradeNumber,
                        Profit = Math.Round(x.Profit, 2),
                        CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2),
                        StrategyId = x.StrategyId
                    }),
                Title = "Equity",
                CanTrackerInterpolatePoints = false,
                TrackerFormatString =   "{1}: {2:0.###}"+ 
                                      "\n{3}: {4:0.###}" +
                                      "\nDateTime: {ExitDateString}"
            };
            
            Model.Series.Add(lineSeries);

            var tradeGroups = Data.GroupBy(x => x.StrategyId).ToList();
            var j = 0;
            var lastIndex = 0;
            foreach (var tradeGroup in tradeGroups)
            {
                if (string.IsNullOrEmpty(tradeGroup.Key))
                    continue;

                if (j > 0)
                {
                    var yLine = new LineAnnotation()
                    {
                        LineStyle = LineStyle.LongDash,
                        X = lastIndex,
                        Type = LineAnnotationType.Vertical,
                        Color = OxyColor.FromRgb(255, 0, 0),
                        StrokeThickness = 2
                    };

                    Model.Annotations.Add(yLine);
                }

                lastIndex += tradeGroup.Count();

                j++;
            }
            Model.InvalidatePlot(true);
            return true;
        }
    }
}