using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Twm.ViewModels.Strategies.Performance.Analysis.Models;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Twm.ViewModels.Strategies.Performance.Analysis.Graphs
{
    public class EquityPortfolioGraph : BaseGraphViewModel
    {
        public EquityPortfolioGraph()
        {
            YTitle = "Profit";
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
                SetMaxMin(ValueAxis, Data.Min(x => x.CumProfit), Data.Max(x => x.CumProfit));
            }

            Model = model;
        }

        public override async Task<bool> LoadData()
        {
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Green,
                Title = "Equity",
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
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "DateTime: {ExitDateString}" +
                                      "\n{3}: {4:0.###}" +
                                      "\nTradeNumber: {TradeNumber}"
            };
            Model.Series.Add(lineSeries);
            Model.InvalidatePlot(true);
            return true;
        }
    }
}