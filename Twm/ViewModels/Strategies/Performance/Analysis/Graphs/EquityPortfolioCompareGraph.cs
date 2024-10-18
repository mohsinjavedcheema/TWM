using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Twm.Core.DataCalc;
using Twm.ViewModels.Strategies.Performance.Analysis.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Graphs
{
    public class EquityPortfolioCompareGraph : BaseGraphViewModel
    {
        public EquityPortfolioCompareGraph()
        {
            XTitle = "Trades";
            YTitle = "Profit";
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
                SetMaxMin(ValueAxis, Data.Min(x => x.CumProfit), Data.Max(x => x.CumProfit));
            }

            Model = model;
            
        }

        public override async Task<bool> LoadData()
        {
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Green,
                ItemsSource = Data.Select(x => new PortfolioTradeInfo()
                    {
                        CumProfit = Math.Round(x.CumProfit, 2),
                        ExitDate = x.ExitTime,
                        TradeNumber = (int)x.TradeNumber,
                        Profit = Math.Round(x.Profit, 2),
                        CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2),
                        StrategyId = x.StrategyId
                    }
                ),
                Title = "Portfolio equity",
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "{0}" +
                                      "\nDateTime: {ExitDateString}" +
                                      "\n{3}: {4:0.###}" +
                                      "\nTradeNumber: {TradeNumber}"
            };
            Model.Series.Add(lineSeries);
            Debug.WriteLine("1");
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
            foreach (var tradeGroup in tradeGroups)
            {
                if (string.IsNullOrEmpty(tradeGroup.Key))
                    continue;

                var trades = tradeGroup.OrderBy(x => x.ExitTime).Select(
                    x => new PortfolioTradeInfo()
                    {
                        ExitDate = x.ExitTime,
                        TradeNumber = (int) x.TradeNumber,
                        Profit = Math.Round(x.Profit, 2),
                        StrategyId = x.StrategyId
                    }
                ).ToList();

                var strategy = Session.Strategies.FirstOrDefault(x => x.LocalId == tradeGroup.Key);

                if (strategy == null && strategies != null)
                {
                    strategy = strategies.FirstOrDefault(x => x.LocalId == tradeGroup.Key);
                }

                string seriesName;

                var startingCapital = 0.0;

                if (strategy != null)
                {
                 

                    if (strategy.Tag != null)
                        seriesName = strategy.Tag + " " + strategy.FullName;
                    else
                        seriesName = strategy.FullName;
                }
                else
                {
                    seriesName = "Equity " + (i + 1);
                }

                var cumProfit = startingCapital;
                foreach (var trade in trades)
                {
                    cumProfit += trade.Profit;
                    trade.CumProfit = cumProfit;
                }

                trades.Insert(0, new PortfolioTradeInfo()
                {
                    ExitDate = Data.Min(x => x.ExitTime),
                    TradeNumber = 0,
                    Profit = startingCapital,
                    CumProfit = startingCapital
                });

                lineSeries = new LineSeries
                {
                    Color = _brushes[i],
                    BrokenLineStyle = LineStyle.DashDot,
                    LineStyle = LineStyle.Dash,
                    ItemsSource = trades.Select(x => new PortfolioTradeInfo()
                        {
                            CumProfit = Math.Round(x.CumProfit, 2),
                            ExitDate = x.ExitDate,
                            TradeNumber = x.TradeNumber,
                            Profit = Math.Round(x.Profit, 2),
                            CurrentDrawDown = Math.Round(x.CurrentDrawDown, 2),
                            StrategyId = x.StrategyId
                        }
                    ),
                    Title = seriesName,
                    CanTrackerInterpolatePoints = false,
                    TrackerFormatString = "{0}" +
                                          "\nDateTime: {ExitDateString}" +
                                          "\n{3}: {4:0.###}" +
                                          "\nTradeNumber: {TradeNumber}"
                };
                Model.Series.Add(lineSeries);
                Debug.WriteLine("6");

                MaxValueY = Math.Max(trades.Max(x => x.CumProfit), MaxValueY);
                MinValueY = Math.Min(trades.Min(x => x.CumProfit), MinValueY);

                i++;
            }

            SetMaxMin(ValueAxis, MinValueY, MaxValueY);
            Model.InvalidatePlot(true);
            return true;
        }

     
    }
}