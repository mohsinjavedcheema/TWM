using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.ViewModels.Strategies.Performance.Analysis.Graphs;
using Twm.ViewModels.Strategies.Validator;


namespace Twm.ViewModels.Strategies.Performance.Analysis
{
    public class AnalysisViewModel : ViewModelBase
    {
        public ObservableCollection<ComboBoxItem> GraphTypes { get; set; }

        private ComboBoxItem _selectedGraphType;

        public ComboBoxItem SelectedGraphType
        {
            get { return _selectedGraphType; }
            set
            {
                if (_selectedGraphType != value)
                {
                    _selectedGraphType = value;
                    if (_selectedGraphType != null)
                    {
                        SelectedGraphViewModel = SelectGraph((GraphType) value.Tag);

                        if (ParentViewModel is OptimizerTestViewModel test)
                        {
                            test.CurrentGraphType = (GraphType) value.Tag;
                        }

                        if (ParentViewModel is OptimizerPeriodViewModel period)
                        {
                            period.Test.CurrentGraphType = (GraphType) value.Tag;
                        }

                        if (ParentViewModel is ValidatorItemViewModel item)
                        {
                            if (item.IsPortfolio)
                                item.CurrentGraphType = (GraphType) value.Tag;
                            else
                            {
                                item.InstrumentList.CurrentGraphType = (GraphType) value.Tag;
                            }
                        }

                        if (IsLoaded)
                        {
                            if (SelectedGraphViewModel.Data == null)
                            {
                                SelectedGraphViewModel.Data = Data;
                                SelectedGraphViewModel.Params = Params;
                                SelectedGraphViewModel.CreatePlotModel();
                                SelectedGraphViewModel.LoadData();
                            }
                            else
                                SelectedGraphViewModel.Model.InvalidatePlot(true);
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        public AnalysisParameters Params { get; set; }

        public List<ITrade> Data { get; set; }

        public bool IsPortfolio { get; set; }


        private BaseGraphViewModel _selectedGraphViewModel;

        public BaseGraphViewModel SelectedGraphViewModel
        {
            get { return _selectedGraphViewModel; }
            set
            {
                if (value != _selectedGraphViewModel)
                {
                    _selectedGraphViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly Dictionary<GraphType, BaseGraphViewModel> _graphViewModels;

        public AnalysisViewModel(bool isPortfolio)
        {
            IsPortfolio = isPortfolio;
            Data = new List<ITrade>();
            FillGraphTypes();
            _graphViewModels = new Dictionary<GraphType, BaseGraphViewModel>();
        }

        public void Clear()
        {
            Data.Clear();
            SelectedGraphViewModel?.Data.Clear();
        }


        private BaseGraphViewModel SelectGraph(GraphType graphType)
        {
            //BaseGraphViewModel graph = null;
            if (!_graphViewModels.TryGetValue(graphType, out var graph))
            {
                switch (graphType)
                {
                    case GraphType.Equity:
                        graph = new EquityGraph();
                        break;

                    case GraphType.PortfolioEquity:
                        graph = new EquityPortfolioGraph();
                        break;

                    case GraphType.PortfolioEquityCompare:
                        graph = new EquityPortfolioCompareGraph();
                        break;

                    case GraphType.DrawDowns:
                        graph = new CumulativeDrawDownGraph();
                        break;

                    case GraphType.DrawDownsCompare:
                        graph = new DrawDownCompareGraph();
                        break;
                }

                _graphViewModels.Add(graphType, graph);
            }

            return graph;
        }

        public void FillGraphTypes()
        {
            GraphTypes = new ObservableCollection<ComboBoxItem>();

            var graphs = EnumHelper.GetValues<GraphType>().ToList();

            foreach (var graph in graphs)
            {
                if (graph == GraphType.Equity && IsPortfolio)
                    continue;


                if ((graph == GraphType.PortfolioEquity || graph == GraphType.PortfolioEquityCompare) && !IsPortfolio)
                    continue;

                if ((graph == GraphType.DrawDownsCompare) && !IsPortfolio)
                    continue;

                ComboBoxItem cb = null;


                Application.Current.Dispatcher.Invoke(() =>
                {
                    cb = new ComboBoxItem
                    {
                        Tag = graph,
                        Content = graph.Description()
                    };
                });
                ;

                GraphTypes.Add(cb);
            }
        }

        public void FetchData(IEnumerable<ITrade> trades, AnalysisParameters parameters)
        {
            Params = parameters;
            Data.AddRange(trades);

            if (Data.Any())
            {
                long minDate = Data.Min(x => x.ExitTime).Ticks;
                long maxDate = Data.Max(x => x.ExitTime).Ticks;

                var avgTime = (maxDate - minDate) / Data.Count;

                if (avgTime == 0)
                    avgTime = (maxDate - Data.Min(x => x.ExitTime).AddDays(-1).Ticks);

                Data.Insert(0,
                    new Trade()
                    {
                        CumProfit = parameters.StartingCapital, Profit = parameters.StartingCapital,
                        ExitTime = new DateTime(minDate - avgTime)
                    });
            }

            if (SelectedGraphViewModel != null)
            {
                if (SelectedGraphViewModel.Data == null)
                {
                    SelectedGraphViewModel.Data = Data;
                }

                SelectedGraphViewModel.CreatePlotModel();
                SelectedGraphViewModel.LoadData();
            }

            IsLoaded = true;
        }
    }
}