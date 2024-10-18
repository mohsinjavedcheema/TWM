using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Twm.ViewModels.Strategies.Performance.Analysis.Models;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Graphs
{
    public class CumulativeDrawDownGraph : BaseGraphViewModel
    {
        public CumulativeDrawDownGraph()
        {
            YTitle = "DrawDown";
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
        }


        public override void CreatePlotModel()
        {
            var model = new ViewResolvingPlotModel { IsLegendVisible = true, LegendPosition = LegendPosition.RightMiddle, LegendPlacement = LegendPlacement.Outside };

            model.Axes.Add(CreateDateTimeAxis());
            model.Axes.Add(CreateValueAxis());

            ValueAxis.Title = YTitle;
            ValueAxis.LabelFormatter = YFormatter;

            if (Data != null && Data.Any())
            {
                SetMaxMin(TimeAxis, DateTimeAxis.ToDouble(Data.Min(x => x.ExitTime)),
                    DateTimeAxis.ToDouble(Data.Max(x => x.ExitTime)));
                SetMaxMin(ValueAxis, Data.Min(x => x.CurrentDrawDown), Data.Max(x => x.CurrentDrawDown));
            }

            Model = model;
        }

        public override async Task<bool> LoadData()
        {

            var lineSeries = new LineSeries
            {
                Color = OxyColors.Red,
                Title = "DrawDown",
                ItemsSource = Data.Select(x => new DrawDownInfo()
                {
                    ExitDate = x.ExitTime,
                    TradeNumber = (int)x.TradeNumber,
                    CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2)
                }),
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "DateTime: {ExitDateString}" +
                                      "\n{3}: {4:0.###}"+
                                      "\nTradeNumber: {TradeNumber}"
            };

            Model.Series.Add(lineSeries);

            if (Params != null && !Params.IsPortfolio)
            {
                var tradeGroups = Data.GroupBy(x => x.StrategyId).ToList();
                var j = 0;
                DateTime lastDate = DateTime.MinValue;
                foreach (var tradeGroup in tradeGroups)
                {
                    if (string.IsNullOrEmpty(tradeGroup.Key))
                        continue;

                    if (j > 0)
                    {
                        var yLine = new LineAnnotation()
                        {
                            LineStyle = LineStyle.LongDash,
                            X = DateTimeAxis.ToDouble(lastDate),
                            Type = LineAnnotationType.Vertical,
                            Color = OxyColor.FromRgb(255, 0, 0),
                            StrokeThickness = 2
                        };

                        Model.Annotations.Add(yLine);
                    }

                    lastDate = tradeGroup.Max(x => x.ExitTime);

                    j++;
                }
            }
            Model.InvalidatePlot(true);
            return true;

        }

    }
}