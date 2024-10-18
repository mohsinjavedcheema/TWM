using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Twm.Chart;
using Twm.Classes;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Common;
using Twm.Core.Helpers;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Instruments;
using Twm.DB.DAL.Repositories.Instruments;
using Twm.Interfaces;
using Twm.ViewModels.Charts;


namespace Twm.Views.Chart
{
   
    public partial class DomView : UserControl
    {

        private DomViewModel _domViewModel;

        private ScrollViewer _lvPricesScrollViewer;

        public static readonly DependencyProperty IsControlEnabledProperty
            = DependencyProperty.Register("IsControlEnabled", typeof(bool), typeof(DomView), new UIPropertyMetadata(null));
        public bool IsControlEnabled
        {
            get { return (bool)GetValue(IsControlEnabledProperty); }
            set { SetValue(IsControlEnabledProperty, value); }
        }


        public DomView()
        {
            InitializeComponent();
            DataContextChanged += ChartView_DataContextChanged;
            
        }

      

       



        private void ChartView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsControlEnabled = false;
            if (e.NewValue != null)
            {
                /*if (DataContext is ChartParamsViewModel chartParamsViewModel)
                {
                    chartParamsViewModel.ConnectionAndTypeChanged += ChartParamsViewModel_ConnectionAndTypeChanged;
                }
                else */if (DataContext is DomViewModel domViewModel)
                {
                    _domViewModel = domViewModel;
                    CreateMenu(domViewModel.DataSeriesParams);
                    IsControlEnabled = true;
                    _domViewModel.OnAutoCentering += _domViewModel_OnAutoCentering;
                    lvPrices.Loaded += LvPrices_Loaded;
                    _domViewModel.ConnectionAndTypeChanged += ChartParamsViewModel_ConnectionAndTypeChanged;
                }
            }

        }


        private void LvPrices_Loaded(object sender, RoutedEventArgs e)
        {
            _lvPricesScrollViewer = lvPrices.GetChildOfType<ScrollViewer>();
            if (_lvPricesScrollViewer?.Template.FindName("PART_VerticalScrollBar", _lvPricesScrollViewer) is ScrollBar scrollBar)
            {
                scrollBar.ValueChanged += delegate
                {
                    _domViewModel.ChangeViewIndex(_lvPricesScrollViewer.VerticalOffset,
                        _lvPricesScrollViewer.ViewportHeight);
                };

                _lvPricesScrollViewer.ScrollChanged += _lvPricesScrollViewer_ScrollChanged;

                _domViewModel.ChangeViewIndex(_lvPricesScrollViewer.VerticalOffset,
                    _lvPricesScrollViewer.ViewportHeight);
                scrollBar.AddHandler(
                    ScrollBar.MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(ListView_MouseLeftButtonDown),
                    true);

                _lvPricesScrollViewer.AddHandler(
                    ListView.MouseWheelEvent,
                    new MouseWheelEventHandler(_lvPricesScrollViewer_MouseWheel),
                    true);


            }
        }


        private void _lvPricesScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = false;
            if (_domViewModel.SelectedInstrument != null)
                _domViewModel.AutoCentering = false;
        }

        private void ListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _domViewModel.AutoCentering = false;
        }

        private void _lvPricesScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            if (_lvPricesScrollViewer == null)
                return;

            _domViewModel.ChangeViewIndex(_lvPricesScrollViewer.VerticalOffset,
                _lvPricesScrollViewer.ViewportHeight);
            if (_domViewModel.AutoCentering)
            {
                ScrollToItem(_domViewModel.PricesViewModel.AskIndex);
            }
        }


        private void _domViewModel_OnAutoCentering(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => ScrollToItem(_domViewModel.PricesViewModel.AskIndex));
        }

        private void ScrollToItem(int index)
        {
            _lvPricesScrollViewer.ScrollToVerticalOffset(index - Math.Truncate(_lvPricesScrollViewer.ViewportHeight / 2));
        }


        private void ChartParamsViewModel_ConnectionAndTypeChanged(object sender, System.EventArgs e)
        {
            if (sender is DataSeriesParamsViewModel dsp)
            {
                CreateMenu(dsp);
            }
        }

        private void CreateMenu(DataSeriesParamsViewModel dsp)
        {
            List<InstrumentListViewModel> instrumentListsVm = new List<InstrumentListViewModel>();

            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new InstrumentListRepository(context);
                var instrumentLists = repository.GetAll().Result;

                foreach (var instrumentList in instrumentLists)
                {
                    instrumentListsVm.Add(new InstrumentListViewModel(instrumentList));
                }
            }
            CreateInstrumentMenu(instrumentListsVm, dsp.SelectedConnection, dsp.SelectedType);
        }

        public void CreateInstrumentMenu(IEnumerable<InstrumentListViewModel> instrumentLists, ConnectionBase connection, string type)
        {
            var instrumnetLists = instrumentLists.Where(x => x.ConnectionId == connection.Id && x.Type == type);
            menu.Items.Clear();


            foreach (var instrumentList in instrumnetLists)
            {
                var miInstrumentList = new MenuItem { Header = instrumentList.Name };

                foreach (var instrument in instrumentList.Instruments)
                {
                    var miInstrument = new MenuItem { Header = instrument.Symbol, Tag = instrument };
                    miInstrument.Click += MiInstrument_Click;
                    miInstrumentList.Items.Add(miInstrument);
                }

                if (miInstrumentList.Items.Count == 0)
                {
                    miInstrumentList.IsEnabled = false;
                }

                menu.Items.Add(miInstrumentList);
            }

        }


        private void MiInstrument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem miInstrument)
            {
                _domViewModel.DataSeriesParams?.SelectSymbol((InstrumentViewModel)miInstrument.Tag);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = true;
        }

    }
}
