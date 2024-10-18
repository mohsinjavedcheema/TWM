using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Charts
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        private readonly ChartViewModel _chartViewModel;

        public ChartWindow()
        {
            InitializeComponent();
        }

        public ChartWindow(ChartViewModel chartViewModel) : this()
        {
            _chartViewModel = chartViewModel;
            App.Charts.Add(_chartViewModel);
            DataContext = _chartViewModel;
            Loaded += ChartWindow_Loaded;
            Closing += ChartWindow_Closing;
            IsVisibleChanged += ChartWindow_IsVisibleChanged;
            KeyDown += ChartWindow_KeyDown;
            Deactivated += ChartWindow_Deactivated;
        }

        private void ChartWindow_Deactivated(object sender, EventArgs e)
        {
            _chartViewModel.Chart.CancelMoveOrderDraw();
        }

        private void ChartWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _chartViewModel.Chart.CancelMoveOrderDraw();
            }
        }

        private void ChartWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _chartViewModel.IsVisible = Visibility == Visibility.Visible;
        }

        private void ChartWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_chartViewModel.Strategy == null)
            {
                var account = Session.Instance.GetAccount(_chartViewModel.Connection);
                if (account == null || account.Positions.All(x => x.Instrument.Symbol != _chartViewModel.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol))
                {
                    _chartViewModel.Destroy();
                    App.Charts.Remove(_chartViewModel);
                    return;
                }
            }
            else
            {
                _chartViewModel.Strategy.ChartWindow = this;
            }
            _chartViewModel.ChartWindow = this;

            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        public void Destroy()
        {
            _chartViewModel.Strategy = null;
            Close();
        }

        public bool IsSstrategyExists()
        {
            return _chartViewModel.Strategy != null;
        }

        private void ChartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _chartViewModel.Chart.OnLoad();
            _chartViewModel.FetchData();

        }
    }
}
