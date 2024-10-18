using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Twm.Chart.Classes;
using Twm.Chart.Elements;

namespace Twm.Chart.Controls
{
    /// <summary>
    /// Логика взаимодействия для PaneControl.xaml
    /// </summary>
    public partial class PaneControl : UserControl
    {
        public string Id { get; set; }

        private Classes.Chart _chart;

        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol", typeof(string), typeof(PaneControl), new PropertyMetadata(default(string)));

        public string Symbol
        {
            get { return (string) GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty TimeVisibleProperty = DependencyProperty.Register(
            "TimeVisible", typeof(Visibility), typeof(PaneControl), new PropertyMetadata(Visibility.Visible));

        public Visibility TimeVisible
        {
            get { return (Visibility) GetValue(TimeVisibleProperty); }
            set { SetValue(TimeVisibleProperty, value); }
        }

        public static readonly DependencyProperty ChartBackgroundProperty = DependencyProperty.Register(
            "ChartBackground", typeof(Brush), typeof(PaneControl), new PropertyMetadata(Brushes.White));

        public Brush ChartBackground
        {
            get { return (Brush) GetValue(ChartBackgroundProperty); }
            set { SetValue(ChartBackgroundProperty, value); }
        }

        public static readonly DependencyProperty IsMainPaneProperty
            = DependencyProperty.Register("IsMainPane", typeof(bool), typeof(PaneControl),
                new UIPropertyMetadata(null));

        public bool IsMainPane
        {
            get { return (bool) GetValue(IsMainPaneProperty); }
            set { SetValue(IsMainPaneProperty, value); }
        }


        public static readonly DependencyProperty VerticalAxisVisibilityProperty
            = DependencyProperty.Register("VerticalAxisVisibility", typeof(Visibility), typeof(PaneControl),
                new UIPropertyMetadata(Visibility.Visible));

        public Visibility VerticalAxisVisibility
        {
            get { return (Visibility) GetValue(VerticalAxisVisibilityProperty); }
            set { SetValue(VerticalAxisVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ScrollBarVisibilityProperty
            = DependencyProperty.Register("ScrollBarVisibility", typeof(Visibility), typeof(PaneControl),
                new UIPropertyMetadata(Visibility.Collapsed, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var paneView = obj as PaneControl;
            if (paneView == null) return;


            if (e.NewValue != null)
            {
                if ((Visibility) e.NewValue == Visibility.Visible)
                {
                    paneView.ScrollBarHeight = new GridLength(25);
                }
                else
                    paneView.ScrollBarHeight = new GridLength(0);
            }


           
        }

        public Visibility ScrollBarVisibility
        {
            get { return (Visibility) GetValue(ScrollBarVisibilityProperty); }
            set { SetValue(ScrollBarVisibilityProperty, value); }
        }


        public static readonly DependencyProperty ScrollBarHeightProperty
            = DependencyProperty.Register("ScrollBarHeight", typeof(GridLength), typeof(PaneControl),
                new UIPropertyMetadata(new GridLength(0)));

        public GridLength ScrollBarHeight
        {
            get { return (GridLength) GetValue(ScrollBarHeightProperty); }
            set { SetValue(ScrollBarHeightProperty, value); }
        }


        public static readonly DependencyPropertyKey OwnerPropertyKey
            = DependencyProperty.RegisterReadOnly("Owner", typeof(object), typeof(PaneControl),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty OwnerProperty = OwnerPropertyKey.DependencyProperty;

        public object Owner
        {
            get { return (object) GetValue(OwnerProperty); }
            protected set { SetValue(OwnerPropertyKey, value); }
        }

        private ContextMenu chartPaneContextMenu;

        public PaneControl()
        {
            InitializeComponent();
            DataContextChanged += ChartView_DataContextChanged;
            chartPaneContextMenu = new ContextMenu();

            Loaded += PaneControl_Loaded;
            Unloaded += PaneControl_Unloaded;    
          
        }

        private void PaneControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown -= HandleKeyDown;
                window.PreviewKeyUp -= HandleKeyUp;
            }
        }

        private void PaneControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown += HandleKeyDown;
                window.PreviewKeyUp += HandleKeyUp;
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ctrlPressed = true;
            }
            if (e.Key == Key.Delete)
            {

                _chart.DeleteDrawObject(_chart.SelectedDraw);
            }
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ctrlPressed = false;
            }
        }

        public void SetOwner(object owner)
        {
            Owner = owner;
        }


        private void ChartView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                _chart = (Classes.Chart) DataContext;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _chart.OnMouseWheel(Keyboard.Modifiers, e.Delta);
        }

        private void OnPanelCandlesContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded || e.NewSize.Width == 0 || _chart.CandlesSource == null || !_chart.CandlesSource.Any())
                return;

            if (e.NewSize.Width != e.PreviousSize.Width)
                _chart.ReCalc_VisibleCandlesRange();
        }

        private void OnMouseMoveInsidePriceChartContainer(object sender, MouseEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(this);
            _chart.CrossLinesPrice = _chart.FormatPrice(_chart.GetPriceFromPoint(mousePosition, this));
        }


        public void RefreshPlots()
        {
            this.paneChart.RefreshPlots();
        }


        private double _lastXPosition;
        private double _lastYPosition;
        private bool _startedHere;
        private double cumDeltaX;
        private double cumDeltaY;
        private bool ctrlPressed;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var parentWindow = Window.GetWindow(this);
            _lastXPosition = e.GetPosition(parentWindow).X;
            _lastYPosition = e.GetPosition(parentWindow).Y;
            if (DataContext is Classes.Chart chart)
            {
                if (chart.ChartControl.MainPane.paneChart.IsDrawing)
                {
                    Cursor = Cursors.Pen;
                    return;
                }
            }
                if (e.OriginalSource is ChartPaneElement)
            {
                _startedHere = true;
            }

            if (e.OriginalSource is ValueTicksElement)
            {
                _startedHereVT = true;
            }


            Cursor = Cursors.Hand;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            Cursor = Cursors.Arrow;
            _startedHere = false;
            _startedHereVT = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
           
        }

      

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
           
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //e.Handled = true;

            if (e.LeftButton == MouseButtonState.Released)
            {
                cumDeltaX = 0;
                cumDeltaY = 0;
                _startedHere = false;
                return;
            }

            if (!_startedHere || e.OriginalSource != paneChart)
            {
                return;
            }

            var parentWindow = Window.GetWindow(this);
            var newPosX = e.GetPosition(parentWindow).X;
            var newPosY = e.GetPosition(parentWindow).Y;
            var deltaX = _lastXPosition - newPosX;
            var deltaY = _lastYPosition - newPosY;
            if (DataContext is Classes.Chart chart)
            {

                cumDeltaY += deltaY;
                if (ctrlPressed)
                {


                    var highValue = chart.VisibleValuesExtremums.CandleValueHigh;
                    var lowValue = chart.VisibleValuesExtremums.CandleValueLow;
                    double chartHeight = chart.ChartControl.MainPane.valueTicksElement.ActualHeight - chart.PriceChartTopMargin - chart.PriceChartTopMargin;
                    double h = chartHeight / (highValue - lowValue);

                    var sign = Math.Sign(cumDeltaY);
                    var diff = (cumDeltaY / (h)) * -1;

                    if (Math.Abs(diff) >= chart.TickSize)
                    {
                        cumDeltaY = (Math.Abs(diff) - chart.TickSize) * h * sign;

                        chart.VisibleValuesExtremums.CandleValueHigh += chart.TickSize * Math.Sign(diff);
                        chart.VisibleValuesExtremums.CandleValueLow += chart.TickSize * Math.Sign(diff);
                        chart.FixedScaleVisibility = Visibility.Visible;
                        chart.FixedScale = true;
                        chart.ChartControl.MainPane.valueTicksElement.Render();
                       // chart.ChartControl.Invalidate();
                    }
                }

                cumDeltaX += deltaX;
                var itemsCount = (int) (cumDeltaX / (chart.CandleWidth + chart.CandleGap));
                cumDeltaX -= itemsCount * (chart.CandleWidth + chart.CandleGap);
                int start_i = Convert.ToInt32(chart.VisibleCandlesRange.Start_i + itemsCount);
                var newRange = IntRange.CreateContainingOnlyStart_i(start_i);
                if (newRange.Start_i >= chart.CandlesSource.Count)
                {
                    newRange.Start_i = chart.CandlesSource.Count;
                }

                if (newRange.Start_i < 0)
                {
                    newRange.Start_i = 0;
                }
                chart.VisibleCandlesRange = newRange;
                
                
                
            }

            _lastXPosition = newPosX;
            _lastYPosition = newPosY;
        }


        private bool _startedHereVT;

        private void ValueTicksElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            var currentPosition = e.GetPosition(parentWindow);
            if (valueTicksElement.PaneWidth < currentPosition.X)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    cumDeltaX = 0;
                    cumDeltaY = 0;
                    _startedHereVT = false;
                    return;
                }

                if (!_startedHereVT)
                {
                    return;
                }

                var deltaY = (_lastYPosition - (int) currentPosition.Y);
                if (DataContext is Classes.Chart chart)
                {
                    //Debug.WriteLine(deltaY);
                    if (deltaY == 0)
                        return;
                    chart.SetNewValueHeight(-(int) deltaY, 5);
                    chart.FixedScaleVisibility = Visibility.Visible;
                    chart.FixedScale = true;
                    chart.ChartControl.Invalidate();
                }

                _lastYPosition = (int) currentPosition.Y;
            }
        }
    }
}