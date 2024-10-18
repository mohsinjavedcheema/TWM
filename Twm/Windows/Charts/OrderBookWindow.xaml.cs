using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Twm.Core.Classes;
using Twm.Interfaces;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Charts
{
    /// <summary>
    /// Логика взаимодействия для OrderBookWindow.xaml
    /// </summary>
    public partial class OrderBookWindow : Window
    {
        private readonly DomViewModel _domeViewModel;

        public OrderBookWindow()
        {
            InitializeComponent();
        }

        public OrderBookWindow(DomViewModel domeViewModel) : this()
        {
            _domeViewModel = domeViewModel;            
            DataContext = _domeViewModel;
            Loaded += ChartWindow_Loaded;
            Closing += ChartWindow_Closing;
            IsVisibleChanged += ChartWindow_IsVisibleChanged;

        }

     

        private void ChartWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //_domeViewModel.IsVisible = Visibility == Visibility.Visible;
        }

        private void ChartWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            _domeViewModel.Unsubscribe();
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        public void Destroy()
        {

            Close();
        }

      
        private void ChartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            

        }
    }
}
