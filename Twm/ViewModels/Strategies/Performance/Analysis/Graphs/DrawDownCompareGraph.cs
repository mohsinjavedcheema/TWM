using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.ViewModels.Strategies.Performance.Analysis.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Graphs
{
    public class DrawDownCompareGraph : BaseGraphViewModel
    {
        public DrawDownCompareGraph()
        {
            YTitle = "DrawDown";
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
        }

        private readonly OxyColor[] _brushes =
        {
            OxyColors.Blue, OxyColors.Red, OxyColors.YellowGreen, OxyColors.DarkBlue, OxyColors.DarkMagenta,
            OxyColors.Magenta,
            OxyColors.DarkOrange
        };

        public override void CreatePlotModel()
        {
            var model = new ViewResolvingPlotModel
            {
                IsLegendVisible = true, LegendPosition = LegendPosition.RightMiddle,
                LegendPlacement = LegendPlacement.Outside,
                LegendMaxWidth = 150,
                LegendSymbolLength = 10
            };


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
            var mainSeries = new LineSeries
            {
                Color = OxyColors.Green,
                ItemsSource = Data.Select(x => new DrawDownInfo()
                {
                    ExitDate = x.ExitTime,
                    TradeNumber = (int) x.TradeNumber,
                    CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2)
                }),
                Title = "Portfolio DrawDown",
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "DateTime: {ExitDateString}" +
                                      "\n{3}: {4:0.###}" +
                                      "\nTradeNumber: {TradeNumber}"
            };

            var tradeGroups = Data.GroupBy(x => x.StrategyId).ToList();
            List<StrategyBase> strategies = null;
            if (Params?.Strategies != null && Params.Strategies.Any())
            {
                strategies = Params.Strategies;
                var strategyOrders = strategies.ToDictionary(x => x.LocalId, y => (int) y.Tag);
                tradeGroups.Sort((x, y) =>
                {
                    if (string.IsNullOrEmpty(x.Key) || string.IsNullOrEmpty(y.Key))
                        return -1;
                    return strategyOrders[x.Key].CompareTo(strategyOrders[y.Key]);
                });
            }

            var i = 0;
            var series = new List<LineSeries>();
            foreach (var tradeGroup in tradeGroups)
            {
                if (string.IsNullOrEmpty(tradeGroup.Key))
                    continue;

                var sp = new SystemPerformance(null, tradeGroup.OrderBy(x => x.ExitTime).ToArray());
                await sp.Calculate(new CancellationToken());

                var trades = sp.Trades.Select(
                    x => new DrawDownInfo()
                    {
                        ExitDate = x.ExitTime,
                        TradeNumber = (int) x.TradeNumber,
                        CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2),
                        StrategyId = x.StrategyId
                    }
                ).ToList();

                var strategy = Session.Strategies.FirstOrDefault(x => x.LocalId == tradeGroup.Key);

                if (strategy == null && strategies != null)
                {
                    strategy = strategies.FirstOrDefault(x => x.LocalId == tradeGroup.Key);
                }

                string seriesName;

                if (strategy != null)
                {
                    if (strategy.Tag != null)
                        seriesName = strategy.Tag + " " + strategy.FullName;
                    else
                        seriesName = strategy.FullName;
                }
                else
                {
                    seriesName = "Drawdown " + (i + 1);
                }

                var lineSeries = new LineSeries
                {
                    Color = _brushes[i],
                    BrokenLineStyle = LineStyle.DashDot,
                    LineStyle = LineStyle.Dash,
                    ItemsSource = trades,
                    Title = seriesName,
                    CanTrackerInterpolatePoints = false,
                    TrackerFormatString = "DateTime: {ExitDateString}" +
                                          "\n{3}: {4:0.###}" +
                                          "\nTradeNumber: {TradeNumber}"
                };
                series.Add(lineSeries);

                if (trades.Any())
                {
                    MaxValueY = Math.Max(trades.Max(x => x.CurrentDrawDown), MaxValueY);
                    MinValueY = Math.Min(trades.Min(x => x.CurrentDrawDown), MinValueY);
                }

                i++;
            }

            SetMaxMin(ValueAxis, MinValueY, MaxValueY);
            Model.Series.Add(mainSeries);
            foreach (var s in series)
            {
                Model.Series.Add(s);
            }

            Model.InvalidatePlot(true);

            return true;
        }
    }
}