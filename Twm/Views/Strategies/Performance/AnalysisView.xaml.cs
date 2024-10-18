using System;
using System.Linq;
using System.Windows.Controls;
using Twm.Core.Enums;
using Twm.ViewModels.Strategies;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.ViewModels.Strategies.Performance.Analysis;
using Twm.ViewModels.Strategies.Validator;

namespace Twm.Views.Strategies.Performance
{
    /// <summary>
    /// Логика взаимодействия для AnalysisView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl
    {
        private AnalysisViewModel _analysisViewModel;

        public AnalysisView()
        {
            InitializeComponent();
            DataContextChanged += AnalysisView_DataContextChanged;
        }

        private void AnalysisView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            /*GraphType? prevGraphType = null;
            if (e.OldValue != null && DataContext is StrategyPerformanceViewModel oldStrategyPerformanceViewModel)
            {
                var oldAnalysisViewModel = oldStrategyPerformanceViewModel.Analysis;
                if (oldAnalysisViewModel.SelectedGraphType.Tag is GraphType graphType)
                {
                    prevGraphType = graphType;
                }
            }*/


            if (DataContext != null && DataContext is StrategyPerformanceViewModel strategyPerformanceViewModel)
            {
                _analysisViewModel = strategyPerformanceViewModel.Analysis;

                if (_analysisViewModel == null)
                    return;



                GraphType? graphType = null;
                if (_analysisViewModel.ParentViewModel is OptimizerTestViewModel test && test.CurrentGraphType != null)
                {
                    graphType = test.CurrentGraphType;
                }
                else if (_analysisViewModel.ParentViewModel is OptimizerPeriodViewModel period &&
                         period.Test.CurrentGraphType != null)
                {
                    graphType = period.Test.CurrentGraphType;
                }
                else if (_analysisViewModel.ParentViewModel is ValidatorItemViewModel item)
                {
                    if (item.IsPortfolio)
                        graphType = item.CurrentGraphType;
                    else
                    {
                        graphType = item.InstrumentList.CurrentGraphType;
                    }
                }


                var graphTypeItem = _analysisViewModel.GraphTypes.FirstOrDefault();
                if (graphType != null)
                {
                    var lastGraphTypeItem = _analysisViewModel.GraphTypes.FirstOrDefault(x => (GraphType) x.Tag == graphType);

                    if (lastGraphTypeItem != null)
                        graphTypeItem = lastGraphTypeItem;
                }

                _analysisViewModel.SelectedGraphType = graphTypeItem;

                /*if (_analysisViewModel.SelectedGraphType == null)
                {
                    _analysisViewModel.SelectedGraphType = _analysisViewModel.GraphTypes.FirstOrDefault();
 
                }*/

                {
                    // if (e.OldValue == null)
                    //   _analysisViewModel.SelectedGraphViewModel.UpdateChart(null);
                    //else
                    {
                        // _analysisViewModel.SelectedGraphViewModel.UpdateProperties();
                    }
                }
            }
        }


        private void SetToolTips()
        {
            /*if (_analysisViewModel.SelectedGraphViewModel is EquityGraph)
            {
                chart.DataTooltip = new TradeTooltip();
            }
            else if (_analysisViewModel.SelectedGraphViewModel is CumulativeDrawDownGraph)
            {
                chart.DataTooltip = new DrawDownTooltip();
            }
            else if (_analysisViewModel.SelectedGraphViewModel is EquityPortfolioGraph)
            {
                chart.DataTooltip = new PortfolioTradeTooltip();
            }
            else
            {
                chart.DataTooltip = new DefaultTooltip();
            }*/
        }


        private void SetToolLegend()
        {
            /*chart.RemoveFromView(chart.ChartLegend);
            if (_analysisViewModel.SelectedGraphViewModel is EquityPortfolioCompareGraph)
            {
                chart.ChartLegend = new PortfolioLegend();
            }
            else
            {
                chart.ChartLegend = new DefaultLegend();
            }*/
        }

        private void SelectedGraphViewModel_OnSetMaxXValue(object sender, System.EventArgs e)
        {
            SetMaxValues();
        }

        private void SetMaxValues()
        {
            /*chart.AxisX[0].MaxValue = _analysisViewModel.SelectedGraphViewModel.MaxValueX;
            chart.AxisY[0].MaxValue = _analysisViewModel.SelectedGraphViewModel.MaxValueY;
            chart.AxisY[0].MinValue = _analysisViewModel.SelectedGraphViewModel.MinValueY;*/

            // chart.InvalidateProperty(CartesianChart.AxisXProperty);
            //  chart.InvalidateProperty(CartesianChart.AxisYProperty);

            try
            {
                //    chart.AxisX[0].Sections.Clear();
            }
            catch (Exception ex)
            {
            }

            //  chart.AxisX[0].Sections?.AddRange(_analysisViewModel.SelectedGraphViewModel.SectionsCollection.ToList());
        }

        private void SelectedGraphViewModel_OnResetZoom(object sender, System.EventArgs e)
        {
            ResetZoom();
        }

        private void ResetZoom()
        {
            /*chart.AxisX[0].MinValue = double.NaN;
            chart.AxisX[0].MaxValue = double.NaN;
            chart.AxisY[0].MinValue = double.NaN;
            chart.AxisY[0].MaxValue = double.NaN;*/
        }
    }
}