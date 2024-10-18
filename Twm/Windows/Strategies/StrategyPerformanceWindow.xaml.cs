using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Strategies;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для StrategyPerformanceWindow.xaml
    /// </summary>
    public partial class StrategyPerformanceWindow : Window
    {

        public bool IsClosed { get; private set; }

        private readonly StrategyPerformanceViewModel _strategyPerformanceViewModel;
        private readonly StrategyViewModel _strategyViewModel;

        private readonly bool IsPortfolio;


        public StrategyPerformanceWindow(StrategyPerformanceViewModel strategyPerformanceViewModel)
        {
            InitializeComponent();
            IsPortfolio = strategyPerformanceViewModel.IsPortfolio;

            if (!IsPortfolio)
                _strategyViewModel = App.Strategies.Strategies.FirstOrDefault(x => x.Id == strategyPerformanceViewModel.Strategy.LocalId);

            _strategyPerformanceViewModel = strategyPerformanceViewModel;

            DataContext = _strategyPerformanceViewModel;

            IsVisibleChanged += StrategyPerformanceWindow_IsVisibleChanged;
        }


        /*public StrategyPerformanceWindow(StrategyBase[] strategies, bool isPortfolio = false)
        {
            InitializeComponent();
            IsPortfolio = isPortfolio;
            var po = new PerformanceOptions()
            {
                IsPortfolio = isPortfolio,
                ExcludeSections = new List<object>() { StrategyPerformanceSection.Chart, StrategyPerformanceSection.RiskLevels}
            };
            if (isPortfolio)
                po.ExcludeSections.Add(StrategyPerformanceSection.Orders);

            _strategyPerformanceViewModel = new StrategyPerformanceViewModel(strategies, null,  po);
            if (!IsPortfolio)
                _strategyViewModel = App.Strategies.Strategies.FirstOrDefault(x => x.Id == strategies[0].LocalId);

            DataContext = _strategyPerformanceViewModel;

            IsVisibleChanged += StrategyPerformanceWindow_IsVisibleChanged;
        }*/

        private void StrategyPerformanceWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue)
                _strategyPerformanceViewModel.Calculate(new CancellationToken());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (!IsPortfolio)
                _strategyViewModel.IsPerformanceVisible = false;
            else
            {
                //Portfolio
                App.Strategies.IsPerformanceVisible = false;
            }
            _strategyPerformanceViewModel?.Dispose();
            IsClosed = true;
        }
    }
}
