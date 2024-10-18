using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.ViewModels.Charts;
using Twm.Windows.Strategies;

namespace Twm.ViewModels.Strategies
{
    public class StrategiesViewModel : DependencyObject, INotifyPropertyChanged
    {
        private StrategyPerformanceWindow _strategyPerformancePortfolioWindow;

        public ObservableCollection<StrategyViewModel> Strategies
        {
            get { return (ObservableCollection<StrategyViewModel>) GetValue(StrategiesProperty); }
            set { SetValue(StrategiesProperty, value); }
        }


        public static readonly DependencyProperty StrategiesProperty =
            DependencyProperty.Register("Strategies", typeof(ObservableCollection<StrategyViewModel>),
                typeof(StrategiesViewModel),
                new UIPropertyMetadata(null));


        private StrategyViewModel _selectedStrategy;

        public StrategyViewModel SelectedStrategy
        {
            get { return _selectedStrategy; }
            set
            {
                if (value != _selectedStrategy)
                {
                    _selectedStrategy = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsPerformanceEnabled");
                    OnPropertyChanged("IsPerformancePortfolioEnabled");
                }
            }
        }

        public bool IsPerformanceEnabled
        {
            get { return (SelectedStrategy != null && SelectedStrategy.Enabled); }
        }

        public bool IsPerformancePortfolioEnabled
        {
            get { return (SelectedStrategy != null && SelectedStrategy.Enabled); }
        }

        private bool _isPerformanceVisible;

        public bool IsPerformanceVisible
        {
            get { return _isPerformanceVisible; }
            set
            {
                if (_isPerformanceVisible != value)
                {
                    _isPerformanceVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        public OperationCommand RemoveCommand { get; set; }

        public OperationCommand PerformanceCommand { get; set; }

        public OperationCommand PerformancePortfolioCommand { get; set; }

        public OperationCommand ShowChartCommand { get; set; }


        public StrategiesViewModel()
        {
            Strategies = new ObservableCollection<StrategyViewModel>();
            Session.Instance.Strategies.CollectionChanged += Strategies_CollectionChanged;

            RemoveCommand = new OperationCommand(RemoveStrategy);
            PerformanceCommand = new OperationCommand(Performance);
            PerformancePortfolioCommand = new OperationCommand(PerformancePortfolio);
            ShowChartCommand = new OperationCommand(ShowChart);
        }

        private void Strategies_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var strategy in e.NewItems.OfType<StrategyBase>())
                {
                    var strategyViewModel = new StrategyViewModel(strategy);
                    Strategies.Add(strategyViewModel);

                    if (strategy.ViewModel is ChartViewModel chartViewModel)
                        chartViewModel.Strategy = strategyViewModel;
                    
                    strategy.Account?.Strategies.Add(strategy);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var idsForRemove = e.OldItems.OfType<StrategyBase>().Select(x => x.LocalId);
                var strategiesForRemove = Strategies.Where(x => idsForRemove.Contains(x.Id)).ToList();
                foreach (var strategy in strategiesForRemove)
                {
                    Strategies.Remove(strategy);
                    strategy.Strategy.Account?.Strategies.Add(strategy.Strategy);
                }
            }
        }

        private void RemoveStrategy(object obj)
        {
            SelectedStrategy.Enabled = false;
            SelectedStrategy.Remove();
            Strategies.Remove(SelectedStrategy);
        }

        private void Performance(object obj)
        {
            SelectedStrategy?.TogglePerformance();
        }

        private void PerformancePortfolio(object obj)
        {
            TogglePortfolioPerformance();
        }


        private void ShowChart(object obj)
        {
            SelectedStrategy?.ShowChartCommand.Execute(null);
        }

        public StrategyBase[] PerformanceStrategies { get; set; }


        private void TogglePortfolioPerformance()
        {
            if (_strategyPerformancePortfolioWindow == null ||
                _strategyPerformancePortfolioWindow.IsClosed)
            {
                PerformanceStrategies = Session.Instance.Strategies.Where(x => x.Enabled).ToArray();


                var po = new PerformanceOptions()
                {
                    IsPortfolio = true,
                    ExcludeSections = new List<object>() { StrategyPerformanceSection.Chart}
                };

                var _strategyPerformanceViewModel = new StrategyPerformanceViewModel(PerformanceStrategies, null, po);



                _strategyPerformancePortfolioWindow = new StrategyPerformanceWindow(_strategyPerformanceViewModel);
                _strategyPerformancePortfolioWindow.Show();
                IsPerformanceVisible = true;
            }
            else
            {
                _strategyPerformancePortfolioWindow.Close();
                IsPerformanceVisible = false;
            }
        }


        public StrategyViewModel GetStrategy(DataCalcContext dataCalcContext)
        {
            var strategy = dataCalcContext.Strategies.FirstOrDefault();
            if (strategy != null)
            {
                return Strategies.FirstOrDefault(x => x.Id == strategy.LocalId);
            }

            return null;
        }


        public void Init()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}