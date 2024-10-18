using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Twm.Chart.Annotations;
using Twm.Chart.Controls;
using Twm.Chart.DrawingTools;
using Twm.Chart.DrawObjects;
using Twm.Chart.Elements;
using Twm.Chart.Interfaces;

namespace Twm.Chart.Classes
{
    public class Chart : DependencyObject, INotifyPropertyChanged
    {
        public IDictionary<int, Brush> BarColors { get; set; } = new Dictionary<Int32, Brush>();
        public IDictionary<int, Brush> BarBackgroundsColors { get; set; } = new Dictionary<Int32, Brush>();

        public IDictionary<int, List<string>> BarTrades { get; set; } = new Dictionary<int, List<string>>();
        public IDictionary<string, TradeDraw> TradeDraws { get; set; } = new Dictionary<string, TradeDraw>();

        public IDictionary<int, List<string>> BarLines { get; set; } = new Dictionary<int, List<string>>();
        public IDictionary<string, BaseLineDraw> LineDraws { get; set; } = new Dictionary<string, BaseLineDraw>();

        public PositionDraw PositionDraw { get; set; }

        public double LastPrice { get; set; }

        public IDictionary<string, OrderDraw> OrderDraws { get; set; } = new Dictionary<string, OrderDraw>();

        public OrderDraw MoveOrderDraw { get; set; }


        private BaseDraw _selectedDraw;
        public BaseDraw SelectedDraw
        {
            get { return _selectedDraw; }
            set
            {
                if (_selectedDraw != value)
                {
                    if (_selectedDraw != null)
                        _selectedDraw.IsSelected = false;
                    _selectedDraw = value;
                    if (_selectedDraw != null)
                        _selectedDraw.IsSelected = true;

                    OnPropertyChanged();
                    OnPropertyChanged("IsDrawSelected");
                    ChartControl.MainPane.paneChart.Render();
                }

            }
        }




        public bool IsDrawSelected
        {
            get { return SelectedDraw != null; }
        }

        public BaseDraw CurrentDraw { get; set; }

        public BaseDraw AdjustDraw { get; set; }

        public ObservableCollection<BaseDraw> DrawObjects { get; set; } = new ObservableCollection<BaseDraw>();



        /// <summary>
        /// Pane collection with collection ArrowDraw object by bar index
        /// </summary>
        public IDictionary<string, IDictionary<int, List<string>>> PaneArrowDraws =
            new Dictionary<string, IDictionary<int, List<string>>>();

        /// <summary>
        /// ArrowDraw objects by unique id
        /// </summary>
        public IDictionary<string, ArrowDraw> ArrowDraws { get; set; } = new Dictionary<string, ArrowDraw>();


        /// <summary>
        /// Pane collection with collection TextDraw object by bar index
        /// </summary>
        public IDictionary<string, IDictionary<int, List<string>>> PaneTextDraws =
            new Dictionary<string, IDictionary<int, List<string>>>();

        /// <summary>
        /// TextDraw objects by unique id
        /// </summary>
        public IDictionary<string, TextDraw> TextDraws { get; set; } = new Dictionary<string, TextDraw>();


        /// <summary>
        /// Pane collection with collection HorizontalLine object by bar index
        /// </summary>
        public IDictionary<string, IDictionary<string, LineDraw>> PaneHLineDraws =
            new Dictionary<string, IDictionary<string, LineDraw>>();


        public ChartControl ChartControl { get; set; }

        public bool NotRender { get; set; }

        public bool IsDestroying { get; set; }

        public int PlotExecutions { get; set; }

        public Draw DrawingTools { get; set; }



        public Visibility DrawTextVisibility
        {
            get
            {
                if (IsTextMode)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }


        public bool IsChartTraderVisible { get; set; }

        public bool IsDrawingToolsVisible { get; set; }
        public Dictionary<PaneControl, ObservableCollection<Plot>> PanePlots { get; set; }

        public readonly object CandlesSourceLock = new object();


        private Visibility _gotoLiveVisibility;

        public Visibility GotoLiveVisibility
        {
            get { return _gotoLiveVisibility; }
            set
            {
                if (_gotoLiveVisibility != value)
                {
                    _gotoLiveVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _fixedScaleVisibility;

        public Visibility FixedScaleVisibility
        {
            get { return _fixedScaleVisibility; }
            set
            {
                if (_fixedScaleVisibility != value)
                {
                    _fixedScaleVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Return all plots on all panes
        /// </summary>
        public IEnumerable<Plot> Plots
        {
            get { return PanePlots.SelectMany(x => x.Value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(nameof(Symbol), typeof(String),
                typeof(Chart), new UIPropertyMetadata(""));

        public String Symbol
        {
            get => (String)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        public static readonly DependencyProperty TickSizeProperty =
            DependencyProperty.Register(nameof(TickSize), typeof(double),
                typeof(Chart), new UIPropertyMetadata(0.0));

        public double TickSize
        {
            get => (double)GetValue(TickSizeProperty);
            set => SetValue(TickSizeProperty, value);
        }

        public ObservableCollection<ICandle> CandlesSource
        {
            get { return (ObservableCollection<ICandle>)GetValue(CandlesSourceProperty); }
            set { SetValue(CandlesSourceProperty, value); }
        }

        /// <summary>Identifies the <see cref="CandlesSource"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CandlesSourceProperty =
            DependencyProperty.Register("CandlesSource", typeof(ObservableCollection<ICandle>), typeof(Chart),
                new UIPropertyMetadata(null, OnCandlesSourceChanged, CoerceCandlesSource));

        public ObservableCollection<ISeries<double>> Series
        {
            get { return (ObservableCollection<ISeries<double>>)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register("Series", typeof(ObservableCollection<ISeries<double>>), typeof(Chart),
                new UIPropertyMetadata(null));


        /// <summary>Gets the range of indexes of candles, currently visible in this chart window.</summary>
        ///<value>The range of indexes of candles, currently visible in this chart window. The default value is <see cref="IntRange.Undefined"/>.</value>
        ///<remarks>
        ///This property defines the part of collection of candles <see cref="CandlesSource"/> that currently visible in the chart window.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public IntRange VisibleCandlesRange
        {
            get { return (IntRange)GetValue(VisibleCandlesRangeProperty); }
            set { SetValue(VisibleCandlesRangeProperty, value); }
        }

        /// <summary>Identifies the <see cref="VisibleCandlesRange"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VisibleCandlesRangeProperty =
            DependencyProperty.Register("VisibleCandlesRange", typeof(IntRange), typeof(Chart),
                new PropertyMetadata(IntRange.Undefined, OnVisibleCanlesRangeChanged, CoerceVisibleCandlesRange));


        private bool _isLogoVisible;

        public bool IsLogoVisible
        {
            get => _isLogoVisible;
            set
            {
                if (value == _isLogoVisible) return;
                _isLogoVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isDarkTheme;

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (value == _isDarkTheme) return;
                _isDarkTheme = value;
                OnPropertyChanged();
            }
        }


        public static readonly DependencyProperty FixedScaleProperty =
            DependencyProperty.Register(nameof(FixedScale), typeof(bool),
                typeof(Chart), new UIPropertyMetadata(false));

        public bool FixedScale
        {
            get => (bool)GetValue(FixedScaleProperty);
            set => SetValue(FixedScaleProperty, value);
        }


        public Brush MarkerTextBrush
        {
            get => _markerTextColor;
            set
            {
                if (Equals(value, _markerTextColor)) return;
                _markerTextColor = value;
                OnPropertyChanged();
            }
        }

        public void UpdateProperty(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible candles range width.</summary>
        ///<value>The the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible candles range width. The default value is <see cref="ModifierKeys.None"/>.</value>
        ///<remarks>
        ///Depending on the keyboard modifier keys the mouse wheel can serve for two functions: scrolling through the candle collection and changing the width of visible candles range. 
        ///You can set up modifier keys for the aforementioned functions by setting the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> and 
        ///<see cref="MouseWheelModifierKeyForCandleWidthChanging"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ModifierKeys MouseWheelModifierKeyForCandleWidthChanging
        {
            get { return (ModifierKeys)GetValue(MouseWheelModifierKeyForCandleWidthChangingProperty); }
            set { SetValue(MouseWheelModifierKeyForCandleWidthChangingProperty, value); }
        }

        /// <summary>Identifies the <see cref="MouseWheelModifierKeyForCandleWidthChanging"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty MouseWheelModifierKeyForCandleWidthChangingProperty =
            DependencyProperty.Register("MouseWheelModifierKeyForCandleWidthChanging", typeof(ModifierKeys),
                typeof(PaneControl), new PropertyMetadata(ModifierKeys.None));

        //--------
        /// <summary>Gets or sets a modifier key that in conjunction with mouse wheel rolling will cause a scrolling through the candles.</summary>
        ///<value>The the modifier key that in conjunction with mouse wheel rolling will cause a scrolling through the candles. The default value is <see cref="ModifierKeys.Control"/>.</value>
        ///<remarks>
        ///Depending on the keyboard modifier keys the mouse wheel can serve for two functions: scrolling through the candle collection and changing the width of visible candles range. 
        ///You can set up modifier keys for the aforementioned functions by setting the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> and 
        ///<see cref="MouseWheelModifierKeyForCandleWidthChanging"/> properties respectively.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VisibleCandlesRangeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ModifierKeys MouseWheelModifierKeyForScrollingThroughCandles
        {
            get { return (ModifierKeys)GetValue(MouseWheelModifierKeyForScrollingThroughCandlesProperty); }
            set { SetValue(MouseWheelModifierKeyForScrollingThroughCandlesProperty, value); }
        }

        /// <summary>Identifies the <see cref="MouseWheelModifierKeyForScrollingThroughCandles"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty MouseWheelModifierKeyForScrollingThroughCandlesProperty =
            DependencyProperty.Register("MouseWheelModifierKeyForScrollingThroughCandles", typeof(ModifierKeys),
                typeof(PaneControl), new PropertyMetadata(ModifierKeys.Control));


        /*//----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible level range height.</summary>
        ///<value>The the modifier key that in conjunction with mouse wheel rolling will cause a change of the visible level range height. The default value is <see cref="ModifierKeys.Control"/>.</value>
        ///<remarks>
        ///Depending on the keyboard modifier keys the mouse wheel can serve for two functions: changing the width and height of visible level range. 
        ///You can set up modifier keys for the aforementioned functions by setting the <see cref="MouseWheelModifierKeyForValueHeightChanging"/> and 
        ///<see cref="MouseWheelModifierKeyForValueHeightChanging"/> properties respectively.
        ///</remarks>
        public ModifierKeys MouseWheelModifierKeyForValueHeightChanging
        {
            get { return (ModifierKeys)GetValue(MouseWheelModifierKeyForValueHeightChangingProperty); }
            set { SetValue(MouseWheelModifierKeyForValueHeightChangingProperty, value); }
        }

        /// <summary>Identifies the <see cref="MouseWheelModifierKeyForValueHeightChanging"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty MouseWheelModifierKeyForValueHeightChangingProperty =
            DependencyProperty.Register("MouseWheelModifierKeyForValueHeightChanging", typeof(ModifierKeys),
                typeof(PaneControl), new PropertyMetadata(ModifierKeys.Control));*/


        static void OnVisibleCanlesRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var chart = (Chart)obj;
            if (chart.IsLoaded && chart.VisibleCandlesRange.Start_i >= 0)
            {
                chart.ReCalc_VisibleCandlesExtremums();

                if (chart.VisibleCandlesRange.Start_i + chart.VisibleCandlesRange.Count ==
                    chart.CandlesSource.Count)
                {
                    chart.GotoLiveVisibility = Visibility.Collapsed;
                }
                else
                {
                    chart.GotoLiveVisibility = Visibility.Visible;
                }
            }
        }

        private static object CoerceCandlesSource(DependencyObject objWithOldDP, object newDPValue)
        {
            var chart = (Chart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            ObservableCollection<ICandle> newValue = (ObservableCollection<ICandle>)newDPValue;

            IntRange vcRange = chart.VisibleCandlesRange;
            if (IntRange.IsUndefined(vcRange))
                chart.lastCenterCandleDateTime = DateTime.MinValue;
            else
            {
                if (chart.CandlesSource != null && (vcRange.Start_i + vcRange.Count) < chart.CandlesSource.Count)
                {
                    int centralCandle_i = (2 * vcRange.Start_i + vcRange.Count) / 2;
                    chart.lastCenterCandleDateTime = chart.CandlesSource[centralCandle_i].t;
                }
                else
                    chart.lastCenterCandleDateTime = DateTime.MaxValue;
            }

            return newValue;
        }


        private static object CoerceVisibleCandlesRange(DependencyObject objWithOldDP, object baseValue)
        {
            var chart = (Chart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            IntRange newValue = (IntRange)baseValue;

            if (IntRange.IsUndefined(newValue))
                return newValue;
            // Это хак для привязки к скроллеру, когда передается только компонента IntRange.Start_i, а компонента IntRange.Count берется из старого значения свойства:
            else if (IntRange.IsContainsOnlyStart_i(newValue))
            {
                if (newValue.Start_i < chart.CandlesSource.Count &&
                    newValue.Start_i + chart.MaxVisibleRangeCandlesCount >= chart.CandlesSource.Count)
                {
                    return new IntRange(newValue.Start_i, chart.CandlesSource.Count - newValue.Start_i);
                }

                if (newValue.Start_i >= chart.CandlesSource.Count)
                {
                    return new IntRange(chart.CandlesSource.Count - 1, 1);
                }

                return new IntRange(newValue.Start_i, chart.MaxVisibleRangeCandlesCount);
            }
            // А это обычная ситуация:
            else
            {
                int newVisibleCandlesStart_i = Math.Max(0, newValue.Start_i);
                int newVisibleCandlesEnd_i = Math.Min(chart.CandlesSource.Count - 1,
                    newValue.Start_i + Math.Max(1, newValue.Count) - 1);
                int maxVisibleCandlesCount = chart.MaxVisibleCandlesCount;

                int newVisibleCandlesCount = newVisibleCandlesEnd_i - newVisibleCandlesStart_i + 1;
                if (newVisibleCandlesCount > maxVisibleCandlesCount)
                {
                    newVisibleCandlesStart_i = newVisibleCandlesEnd_i - maxVisibleCandlesCount + 1;
                    newVisibleCandlesCount = maxVisibleCandlesCount;
                }

                return new IntRange(newVisibleCandlesStart_i, newVisibleCandlesCount);
            }
        }


        int timeFrame;
        public Func<double> PaneWidth { get; set; }

        /// <summary>Gets the automatically determined timeframe of this chart in minutes.</summary>
        ///<value>The automatically determined timeframe of this chart in minutes.</value>
        ///<remarks>
        ///This value is recalculated every time the <see cref="CandlesSource"/> property is changed.
        ///</remarks>
        public int TimeFrame
        {
            get { return timeFrame; }
            private set
            {
                if (value == timeFrame) return;
                timeFrame = value;
                OnPropertyChanged();
            }
        }


        int maxNumberOfDigitsAfterPointInPrice = 0;

        /// <summary>Gets the maximum number of fractional digits in a price and volume for the current candle collection.</summary>
        ///<value>The maximum number of fractional digits in a price and volume for the current candle collection.</value>
        ///<remarks>
        ///This property is recalculated every time the <see cref="CandlesSource"/> property is changed.
        ///</remarks>
        public int MaxNumberOfDigitsAfterPointInPrice
        {
            get { return maxNumberOfDigitsAfterPointInPrice; }
            private set
            {
                if (value == maxNumberOfDigitsAfterPointInPrice) return;
                maxNumberOfDigitsAfterPointInPrice = value;
                OnPropertyChanged();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ValuesExtremums _visibleValuesExtremums;

        ///<summary>Gets the Low and High of the visible candles in vector format (Low,High).</summary>
        ///<value>The Low and High of the visible candles in vector format (Low,High).</value>
        ///<remarks>
        ///<para>The visible candles are those that fall inside the visible candles range, which is determined by the <see cref="VisibleCandlesRange"/> property.</para>
        ///The Low of a set of candles is a minimum Low value of this candles. The High of a set of candles is a maximum High value of this candles.
        ///</remarks>
        public ValuesExtremums VisibleValuesExtremums
        {
            get { return _visibleValuesExtremums; }
            private set
            {
                _visibleValuesExtremums = value;
                OnPropertyChanged();
            }
        }

        private bool isCrossLinesVisible;

        public bool IsCrossLinesVisible
        {
            get => isCrossLinesVisible;
            set
            {
                if (isCrossLinesVisible == value)
                {
                    return;
                }

                isCrossLinesVisible = value;
                OnPropertyChanged();
            }
        }

        public void ResetDrawMode()
        {
            ToogleAllButtons("Clear");
        }

        private void ToogleAllButtons(string propertyName)
        {
            if (propertyName != nameof(IsLineMode))
            {
                _isLineMode = false;
                OnPropertyChanged("IsLineMode");
            }
            if (propertyName != nameof(IsHLineMode))
            {
                _isHLineMode = false;
                OnPropertyChanged("IsHLineMode");
            }
            if (propertyName != nameof(IsVLineMode))
            {
                _isVLineMode = false;
                OnPropertyChanged("IsVLineMode");
            }
            if (propertyName != nameof(IsTextMode))
            {
                _isTextMode = false;
                OnPropertyChanged("IsTextMode");
            }
            if (propertyName != nameof(IsRayMode))
            {
                _isRayMode = false;
                OnPropertyChanged("IsRayMode");
            }
            if (propertyName != nameof(IsRectMode))
            {
                _isRectMode = false;
                OnPropertyChanged("IsRectMode");
            }
            if (propertyName != nameof(IsRulerMode))
            {
                _isRulerMode = false;
                OnPropertyChanged("IsRulerMode");
            }
            if (propertyName != nameof(IsRiskMode))
            {
                _isRiskMode = false;
                OnPropertyChanged("IsRiskMode");
            }
        }

        public bool IsDrawingMode
        {

            get { return _isLineMode || _isHLineMode || _isVLineMode || _isTextMode || IsRayMode || IsRulerMode; }
        }

        private bool _isLineMode;
        public bool IsLineMode
        {
            get => _isLineMode;
            set
            {
                ToogleAllButtons(nameof(IsLineMode));
                if (_isLineMode == value)
                {
                    return;
                }

                _isLineMode = value;

                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }

        private bool _isHLineMode;
        public bool IsHLineMode
        {
            get => _isHLineMode;
            set
            {
                ToogleAllButtons(nameof(IsHLineMode));
                if (_isHLineMode == value)
                {
                    return;
                }

                _isHLineMode = value;

                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }

        private bool _isRayMode;
        public bool IsRayMode
        {
            get => _isRayMode;
            set
            {
                ToogleAllButtons(nameof(IsRayMode));
                if (_isRayMode == value)
                {
                    return;
                }

                _isRayMode = value;
                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }


        private bool _isRectMode;
        public bool IsRectMode
        {
            get => _isRectMode;
            set
            {
                ToogleAllButtons(nameof(IsRectMode));
                if (_isRectMode == value)
                {
                    return;
                }

                _isRectMode = value;
                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }


        private bool _isVLineMode;
        public bool IsVLineMode
        {
            get => _isVLineMode;
            set
            {
                ToogleAllButtons(nameof(IsVLineMode));
                if (_isVLineMode == value)
                {
                    return;
                }

                _isVLineMode = value;
                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }

        private bool _isTextMode;
        public bool IsTextMode
        {
            get => _isTextMode;
            set
            {
                ToogleAllButtons(nameof(IsTextMode));
                if (_isTextMode == value)
                {
                    return;
                }

                _isTextMode = value;
                OnPropertyChanged();
                UpdatModeVisibilities();
            }
        }


        private bool _isRulerMode;
        public bool IsRulerMode
        {
            get => _isRulerMode;
            set
            {
                ToogleAllButtons(nameof(IsRulerMode));
                if (_isRulerMode == value)
                {
                    return;
                }

                _isRulerMode = value;
                UpdatModeVisibilities();
            }
        }

        private bool _isRiskMode;
        public bool IsRiskMode
        {
            get => _isRiskMode;
            set
            {
                ToogleAllButtons(nameof(IsRiskMode));
                if (_isRiskMode == value)
                {
                    return;
                }

                _isRiskMode = value;
                UpdatModeVisibilities();
            }
        }



        private Color _drawingColor;
        public Color DrawingColor
        {
            get { return _drawingColor; }
            set
            {
                if (_drawingColor != value)
                {
                    _drawingColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _drawingWidth;
        public int DrawingWidth
        {
            get { return _drawingWidth; }
            set
            {
                if (_drawingWidth != value)
                {
                    _drawingWidth = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _drawingSize;
        public int DrawingSize
        {
            get { return _drawingSize; }
            set
            {
                if (_drawingSize != value)
                {
                    _drawingSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _drawingText;

        public string DrawingText
        {
            get { return _drawingText; }
            set
            {
                if (_drawingText != value)
                {
                    _drawingText = value;
                    OnPropertyChanged();

                }
            }
        }

        private void UpdatModeVisibilities()
        {
            OnPropertyChanged("DrawTextVisibility"); ;
        }



        private Point _axisPosition;

        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public Point AxisPosition
        {
            get => _axisPosition;
            set
            {
                if (_axisPosition == value)
                {
                    return;
                }

                _axisPosition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMouseInChartArea));
            }
        }

        private Point _currentMousePosition;

        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public Point CurrentMousePosition
        {
            get => _currentMousePosition;
            set
            {
                if (_currentMousePosition == value)
                {
                    return;
                }

                _currentMousePosition = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMouseInChartArea));
                OnPropertyChanged(nameof(CrossHairPriceTextBoxTop));
                OnPropertyChanged(nameof(CrossHairTimeTextBoxLeft));
            }
        }


        private double _crossHairPriceTextBoxHeight;

        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public double CrossHairPriceTextBoxHeight
        {
            get => _crossHairPriceTextBoxHeight;
            set
            {
                if (_crossHairPriceTextBoxHeight == value)
                {
                    return;
                }

                _crossHairPriceTextBoxHeight = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CrossHairPriceTextBoxTop));
            }
        }


        private double _crossHairTimeTextBoxWidth;

        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public double CrossHairTimeTextBoxWidth
        {
            get => _crossHairTimeTextBoxWidth;
            set
            {
                if (_crossHairTimeTextBoxWidth == value)
                {
                    return;
                }

                _crossHairTimeTextBoxWidth = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CrossHairTimeTextBoxLeft));
            }
        }

        public double CrossHairPriceTextBoxTop
        {
            get
            {
                return CurrentMousePosition.Y - (CrossHairPriceTextBoxHeight / 2);
            }
        }

        public double CrossHairTimeTextBoxLeft
        {
            get
            {
                return CurrentMousePosition.X - (CrossHairTimeTextBoxWidth / 2);
            }
        }




        public bool IsMouseInChartArea
        {
            get => _currentMousePosition.X < _axisPosition.X && (_currentMousePosition.Y < _axisPosition.Y && _currentMousePosition.Y > PriceChartTopMargin);
        }

        private int MaxVisibleCandlesCount
        {
            get { return (int)(ChartControl.ActualWidth / 2); }
        }

        public int MaxVisibleRangeCandlesCount
        {
            get
            {
                var actualWidth = ChartControl.MainPane.ActualWidth - PriceAxisWidth;
                if (actualWidth == 0 || CandlesSource == null)
                {
                    return 0;
                }


                return (int)(actualWidth / (CandleWidth + CandleGap));
            }
        }


        #region PRICE CHART PROPERTIES **************************************************************************************************************************

        /// <summary>Gets or sets the top margin for the price chart.</summary>
        ///<value>The top margin of the price chart, in device-independent units. The default is determined by the <see cref="DefaultPriceChartTopMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the price chart inside its area by setting the <see cref="PriceChartTopMargin"/> and <see cref="PriceChartBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceChartTopMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultPriceChartTopMargin">DefaultPriceChartTopMargin</seealso>
        ///<seealso cref = "PriceChartBottomMargin">PriceChartBottomMargin</seealso>
        public double PriceChartTopMargin
        {
            get { return (double)GetValue(PriceChartTopMarginProperty); }
            set { SetValue(PriceChartTopMarginProperty, value); }
        }

        /// <summary>Identifies the <see cref="PriceChartTopMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceChartTopMarginProperty =
            DependencyProperty.Register("PriceChartTopMargin", typeof(double), typeof(Chart),
                new PropertyMetadata(DefaultPriceChartTopMargin));

        ///<summary>Gets the default value for the PriceChartTopMargin property.</summary>
        ///<value>The default value for the PriceChartTopMargin property: <c>15.0</c>.</value>
        ///<seealso cref = "PriceChartTopMargin">PriceChartTopMargin</seealso>
        public static double DefaultPriceChartTopMargin
        {
            get { return 15.0; }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the bottom margin for the price chart.</summary>
        ///<value>The bottom margin of the price chart, in device-independent units. The default is determined by the <see cref="DefaultPriceChartBottomMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the price chart inside its area by setting the <see cref="PriceChartTopMargin"/> and <see cref="PriceChartBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceChartBottomMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "DefaultPriceChartBottomMargin">DefaultPriceChartBottomMargin</seealso>
        ///<seealso cref = "PriceChartTopMargin">PriceChartTopMargin</seealso>
        public double PriceChartBottomMargin
        {
            get { return (double)GetValue(PriceChartBottomMarginProperty); }
            set { SetValue(PriceChartBottomMarginProperty, value); }
        }

        /// <summary>Identifies the <see cref="PriceChartBottomMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceChartBottomMarginProperty =
            DependencyProperty.Register("PriceChartBottomMargin", typeof(double), typeof(Chart),
                new PropertyMetadata(DefaultPriceChartBottomMargin));

        ///<summary>Gets the default value for the PriceChartBottomMargin property.</summary>
        ///<value>The default value for the PriceChartBottomMargin property: <c>15.0</c>.</value>
        ///<seealso cref = "PriceChartBottomMargin">PriceChartBottomMargin</seealso>
        public static double DefaultPriceChartBottomMargin
        {
            get { return 15.0; }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty ChartBackgroundProperty = DependencyProperty.Register(
            "ChartBackground", typeof(Brush), typeof(Chart), new PropertyMetadata(Brushes.White));

        public Brush ChartBackground
        {
            get { return (Brush)GetValue(ChartBackgroundProperty); }
            set { SetValue(ChartBackgroundProperty, value); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the Brush that specifies how the body of the bearish candle is painted.</summary>
        ///<value>The brush to paint the bodies of the bearish candles. The default value is determined by the <see cref="DefaultBearishCandleFill"/> property.</value>
        ///<remarks> 
        /// Usually candles are separated into two types - Bullish and Bearish. The Bullish candle has its Close price higher than its Open price. All other candles are Bearish. 
        /// To visualize such a separation usually the Bearish and Bullish candles are painted in different Fill and Stroke (outline) colors. 
        /// To set the Brush for the candle outline (the tails and the body outline) use the <see cref="BullishCandleStroke"/> and <see cref="BearishCandleStroke"/> properties for the bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BearishCandleFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "BullishCandleFill">BearishCandleFill</seealso>
        ///<seealso cref = "BullishCandleStroke">BearishCandleStroke</seealso>
        ///<seealso cref = "BearishCandleStroke">BullishCandleStroke</seealso>
        public Brush BearishCandleFill
        {
            get { return (Brush)GetValue(BearishCandleFillProperty); }
            set { SetValue(BearishCandleFillProperty, value); }
        }

        /// <summary>Identifies the <see cref="BearishCandleFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BearishCandleFillProperty =
            DependencyProperty.Register("BearishCandleFill", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultBearishCandleFill));

        ///<summary>Gets the default value for the BearishCandleFill property.</summary>
        ///<value>The default value for the BearishCandleFill property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        public static Brush DefaultBearishCandleFill
        {
            get { return (Brush)(new SolidColorBrush(Colors.Red)).GetCurrentValueAsFrozen(); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the Brush that specifies how the body of the bearish candle is painted.</summary>
        ///<value>The brush to paint the bodies of the bearish candles. The default value is determined by the <see cref="DefaultBullishCandleFill"/> property.</value>
        ///<remarks> 
        /// Usually candles are separated into two types - Bullish and Bearish. The Bullish candle has its Close price higher than its Open price. All other candles are Bearish. 
        /// To visualize such a separation usually the Bearish and Bullish candles are painted in different Fill and Stroke (outline) colors. 
        /// To set the Brush for the candle outline (the tails and the body outline) use the <see cref="BullishCandleStroke"/> and <see cref="BearishCandleStroke"/> properties for the bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BullishCandleFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        ///<seealso cref = "BullishCandleStroke">BearishCandleStroke</seealso>
        ///<seealso cref = "BearishCandleStroke">BullishCandleStroke</seealso>
        public Brush BullishCandleFill
        {
            get { return (Brush)GetValue(BullishCandleFillProperty); }
            set { SetValue(BullishCandleFillProperty, value); }
        }

        /// <summary>Identifies the <see cref="BullishCandleFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BullishCandleFillProperty =
            DependencyProperty.Register("BullishCandleFill", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultBullishCandleFill));

        ///<summary>Gets the default value for the BearishCandleFill property.</summary>
        ///<value>The default value for the BearishCandleFill property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BullishCandleFill">BearishCandleFill</seealso>
        public static Brush DefaultBullishCandleFill
        {
            get { return (Brush)(new SolidColorBrush(Colors.LimeGreen)).GetCurrentValueAsFrozen(); }
        }


        public static readonly DependencyProperty IndicatorSeparatorWidthProperty = DependencyProperty.Register(
            "IndicatorSeparatorWidth", typeof(int), typeof(Chart), new PropertyMetadata(default(int)));

        public int IndicatorSeparatorWidth
        {
            get { return (int)GetValue(IndicatorSeparatorWidthProperty); }
            set { SetValue(IndicatorSeparatorWidthProperty, value); }
        }

        public static readonly DependencyProperty IndicatorSeparatorColorProperty = DependencyProperty.Register(
            "IndicatorSeparatorColor", typeof(Brush), typeof(Chart), new PropertyMetadata(default(Brush)));

        public Brush IndicatorSeparatorColor
        {
            get { return (Brush)GetValue(IndicatorSeparatorColorProperty); }
            set { SetValue(IndicatorSeparatorColorProperty, value); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the Brush that specifies how the outline of the bullish candle is painted.</summary>
        ///<value>The Brush to paint the tails and the body outline of the bullish candles. The default value is determined by the <see cref="DefaultBullishCandleStroke"/> property.</value>
        ///<remarks> 
        /// Usually candles are separated into two types - Bullish and Bearish. The Bullish candle has its Close price higher than its Open price. All other candles are Bearish. 
        /// To visualize such a separation usually the Bearish and Bullish candles are painted in different Fill and Stroke (outline) colors. 
        /// To set the Brush for the candle body fill use the <see cref="BullishCandleFill"/> and <see cref="BearishCandleFill"/> properties for the bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BullishCandleStrokeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "BearishCandleStroke">BearishCandleStroke</seealso>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        ///<seealso cref = "BullishCandleFill">BullishCandleFill</seealso>
        public Brush BullishCandleStroke
        {
            get { return (Brush)GetValue(BullishCandleStrokeProperty); }
            set { SetValue(BullishCandleStrokeProperty, value); }
        }

        /// <summary>Identifies the <see cref="BullishCandleStroke"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BullishCandleStrokeProperty =
            DependencyProperty.Register("BullishCandleStroke", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultBullishCandleStroke));

        ///<summary>Gets the default value for the BullishCandleStroke property.</summary>
        ///<value>The default value for the BullishCandleStroke property: <c>Brushes.Green</c>.</value>
        ///<seealso cref = "BullishCandleStroke">BullishCandleStroke</seealso>
        public static Brush DefaultBullishCandleStroke
        {
            get { return (Brush)(new SolidColorBrush(Colors.LimeGreen)).GetCurrentValueAsFrozen(); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the Brush that specifies how the outline of the bearish candle is painted.</summary>
        ///<value>The Brush to paint the tails and the body outline of the bearish candles. The default value is determined by the <see cref="DefaultBearishCandleStroke"/> property.</value>
        ///<remarks> 
        /// Usually candles are separated into two types - Bullish and Bearish. The Bullish candle has its Close price higher than its Open price. All other candles are Bearish. 
        /// To visualize such a separation usually the Bearish and Bullish candles are painted in different Fill and Stroke (outline) colors. 
        /// To set the Brush for the candle body fill use the <see cref="BullishCandleFill"/> and <see cref="BearishCandleFill"/> properties for the bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BearishCandleStrokeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "BullishCandleStroke">BullishCandleStroke</seealso>
        ///<seealso cref = "BullishCandleFill">BullishCandleFill</seealso>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        public Brush BearishCandleStroke
        {
            get { return (Brush)GetValue(BearishCandleStrokeProperty); }
            set { SetValue(BearishCandleStrokeProperty, value); }
        }

        /// <summary>Identifies the <see cref="BearishCandleStroke"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BearishCandleStrokeProperty =
            DependencyProperty.Register("BearishCandleStroke", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultBearishCandleStroke));

        ///<summary>Gets the default value for the BearishCandleStroke property.</summary>
        ///<value>The default value for the BearishCandleStroke property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishCandleStroke">BearishCandleStroke</seealso>
        public static Brush DefaultBearishCandleStroke
        {
            get { return (Brush)(new SolidColorBrush(Colors.Red)).GetCurrentValueAsFrozen(); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the Brush that specifies how the outline of the bearish candle is painted.</summary>
        ///<value>The Brush to paint the tails and the body outline of the bearish candles. The default value is determined by the <see cref="DefaultWickCandleStroke"/> property.</value>
        ///<remarks> 
        /// Usually candles are separated into two types - Bullish and Bearish. The Bullish candle has its Close price higher than its Open price. All other candles are Bearish. 
        /// To visualize such a separation usually the Bearish and Bullish candles are painted in different Fill and Stroke (outline) colors. 
        /// To set the Brush for the candle body fill use the <see cref="BullishCandleFill"/> and <see cref="BearishCandleFill"/> properties for the bullish and bearish candles respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="WickCandleStrokeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "BullishCandleStroke">BullishCandleStroke</seealso>
        ///<seealso cref = "BearishCandleStroke">BearishCandleStroke</seealso>
        ///<seealso cref = "BullishCandleFill">BullishCandleFill</seealso>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        public Brush WickCandleStroke
        {
            get { return (Brush)GetValue(WickCandleStrokeProperty); }
            set { SetValue(WickCandleStrokeProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor", typeof(Brush), typeof(Chart), new PropertyMetadata(default(Brush)));

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        /// <summary>Identifies the <see cref="WickCandleStroke"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty WickCandleStrokeProperty =
            DependencyProperty.Register("WickCandleStroke", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultWickCandleStroke));

        ///<summary>Gets the default value for the WickCandleStroke property.</summary>
        ///<value>The default value for the WickCandleStroke property: <c>Brushes.Black</c>.</value>
        ///<seealso cref = "WickCandleStroke">WickCandleStroke</seealso>
        public static Brush DefaultWickCandleStroke
        {
            get { return (Brush)(new SolidColorBrush(Colors.Black)).GetCurrentValueAsFrozen(); }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the initial candle width.</summary>
        ///<value>The initial width of the candle, in device-independent units (1/96th inch per unit). 
        ///The default is determined by the <see cref="DefaultInitialCandleWidth"/> value.</value>
        ///<remarks>Initially the width of a candle <see cref="CandleWidth"/> is equal to this property value, but then the <see cref="CandleWidth"/> property value changes due to user's manipulations.</remarks>
        ///<seealso cref = "DefaultInitialCandleWidth">DefaultInitialCandleWidth</seealso>
        ///<seealso cref = "CandleWidth">CandleWidth</seealso>
        public double InitialCandleWidth { get; set; }

        ///<summary>Gets the default value for the InitialCandleWidth property.</summary>
        ///<value>The default value for the <see cref="InitialCandleWidth"/> property, in device-independent units: <c>3.0</c>.</value>
        ///<seealso cref = "InitialCandleWidth">InitialCandleWidth</seealso>
        ///<seealso cref = "CandleWidth">CandleWidth</seealso>
        public readonly double DefaultInitialCandleWidth = 5.0;

        private double candleWidth;

        /// <summary>Gets the width of the candle.</summary>
        ///<value>The candle width, in device-independent units (1/96th inch per unit).</value>
        ///<remarks>Initially after loading this property value is equal to the <see cref="InitialCandleWidth"/>, but then it changes due to user's manipulations.</remarks>
        ///<seealso cref = "InitialCandleWidth">InitialCandleWidth</seealso>
        ///<seealso cref = "DefaultInitialCandleWidth">DefaultInitialCandleWidth</seealso>
        ///<seealso cref = "CandleGap">CandleGap</seealso>
        public double CandleWidth
        {
            get { return candleWidth; }
            private set
            {
                if (candleWidth == value) return;
                candleWidth = value;
                OnPropertyChanged();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the initial gap between adjacent candles.</summary>
        ///<value>The initial gap between adjacent candles, in device-independent units (1/96th inch per unit). 
        ///The default is determined by the <see cref="DefaultInitialCandleGap"/> value.</value>
        ///<remarks>Initially the gap between candles <see cref="CandleGap"/> is equal to this property value, but then the <see cref="CandleGap"/> property value changes due to user's manipulations.</remarks>
        ///<seealso cref = "DefaultInitialCandleGap">DefaultInitialCandleGap</seealso>
        ///<seealso cref = "CandleGap">CandleGap</seealso>
        public double InitialCandleGap { get; set; }

        ///<summary>Gets the default value for the InitialCandleGap property.</summary>
        ///<value>The default value for the <see cref="InitialCandleGap"/> property, in device-independent units: <c>1.0</c>.</value>
        ///<seealso cref = "InitialCandleGap">InitialCandleGap</seealso>
        ///<seealso cref = "CandleGap">CandleGap</seealso>
        public readonly double DefaultInitialCandleGap = 1.0;

        double candleGap;

        /// <summary>Gets the horizontal gap between adjacent candles.</summary>
        ///<value>The horizontal gap between adjacent candles, in device-independent units (1/96th inch per unit).</value>
        ///<remarks>Initially after loading this property value is equal to the <see cref="InitialCandleGap"/>, but then it changes due to user's manipulations.</remarks>
        ///<seealso cref = "DefaultInitialCandleGap">DefaultInitialCandleGap</seealso>
        ///<seealso cref = "InitialCandleGap">InitialCandleGap</seealso>
        ///<seealso cref = "CandleWidth">CandleWidth</seealso>
        public double CandleGap
        {
            get { return candleGap; }
            private set
            {
                if (candleGap == value) return;
                candleGap = value;
                OnPropertyChanged();
            }
        }

        /*private T FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T dependencyObject)
                    {
                        return dependencyObject;
                    }

                    return FindVisualChildren<T>(child);
                }
            }

            return null;
        }*/


        //----------------------------------------------------------------------------------------------------------------------------------
        private bool ReCalc_CandleWidthAndGap(int new_VisibleCandlesCount)
        {
            if (new_VisibleCandlesCount <= 0) return false;


            var actualRenderWidth = /*(ChartControl.MainPane.paneChart.RenderSize.Width -*/
                (VisibleCandlesRange.Count * (CandleWidth + CandleGap));

            double new_ActualWidth = actualRenderWidth;
            if (new_ActualWidth == 0)
            {
                CandleGap = 0.0;
                CandleWidth = 0.0;
                return false;
            }

            double new_candleWidth = CandleWidth;
            while (new_VisibleCandlesCount * (new_candleWidth + 1.0) + 1.0 > new_ActualWidth)
            {
                if (Math.Round(new_candleWidth) < 3.0)
                    return false;
                else
                    new_candleWidth -= 2.0;
            }

            while (new_VisibleCandlesCount * (new_candleWidth + 3.0) + 1.0 < new_ActualWidth)
                new_candleWidth += 2.0;

            CandleGap = (new_ActualWidth - new_VisibleCandlesCount * new_candleWidth - 1.0) / new_VisibleCandlesCount;
            CandleWidth = new_candleWidth;
            return true;
        }


        public Brush TradeBuyArrowFill
        {
            get { return (Brush)GetValue(TradeBuyArrowFillProperty); }
            set { SetValue(TradeBuyArrowFillProperty, value); }
        }

        /// <summary>Identifies the <see cref="TradeBuyArrowFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty TradeBuyArrowFillProperty =
            DependencyProperty.Register("TradeBuyArrowFill", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultTradeBuyArrowFill));

        ///<summary>Gets the default value for the TradeBuyArrowFill property.</summary>
        ///<value>The default value for the TradeBuyArrowFill property: <c>Brushes.Blue</c>.</value>
        ///<seealso cref = "TradeBuyArrowFill">BearishCandleFill</seealso>
        public static Brush DefaultTradeBuyArrowFill
        {
            get { return (Brush)(new SolidColorBrush(Colors.Blue)).GetCurrentValueAsFrozen(); }
        }


        public Brush TradeSellArrowFill
        {
            get { return (Brush)GetValue(TradeSellArrowFillProperty); }
            set { SetValue(TradeSellArrowFillProperty, value); }
        }

        /// <summary>Identifies the <see cref="TradeSellArrowFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty TradeSellArrowFillProperty =
            DependencyProperty.Register("TradeSellArrowFill", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultTradeSellArrowFill));

        ///<summary>Gets the default value for the TradeSellArrowFill property.</summary>
        ///<value>The default value for the TradeSellArrowFill property: <c>Brushes.Magenta</c>.</value>
        ///<seealso cref = "TradeSellArrowFill">TradeSellArrowFill</seealso>
        public static Brush DefaultTradeSellArrowFill
        {
            get { return (Brush)(new SolidColorBrush(Colors.Magenta)).GetCurrentValueAsFrozen(); }
        }

        #endregion **********************************************************************************************************************************************

        #region COMMON PROPERTIES FOR THE PRICE AXIS AND THE TIME AXIS

        ///<summary>Gets or sets a color of lines, ticks and its labels for the time axis, the price axis and the volume axis.</summary>
        ///<value>The color of lines, ticks and its labels for the time axis, the price axis and the volume axis. The default is determined by the <see cref="DefaultAxisTickColor"/> value.</value>
        ///<remarks> 
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="AxisTickColorProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush AxisTickColor
        {
            get { return (Brush)GetValue(AxisTickColorProperty); }
            set { SetValue(AxisTickColorProperty, value); }
        }

        /// <summary>Identifies the <see cref="AxisTickColor">AxisTickColor</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty AxisTickColorProperty =
            DependencyProperty.RegisterAttached("AxisTickColor", typeof(Brush), typeof(Chart),
                new FrameworkPropertyMetadata(Settings.DefaultAxisTickColor,
                    FrameworkPropertyMetadataOptions.Inherits));

        #endregion **********************************************************************************************************************************************

        #region PROPERTIES OF THE PRICE AXIS (AND OF THE VOLUME AXIS, WHICH DOESN'T HAVE ITS OWN PROPERTIES) ****************************************************

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the font size of the tick labels for the price and volume axis.</summary>
        ///<value>The font size of the tick labels for the price and volume axis. The default is determined by the <see cref="DefaultPriceTickFontSize"/> value.</value>
        ///<remarks>
        /// The volume axis doesn't have its own appearance properties. Therefore, the volume axis appearance depends on price axis properties. 
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="PriceTickFontSizeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double PriceTickFontSize
        {
            get { return (double)GetValue(PriceTickFontSizeProperty); }
            set { SetValue(PriceTickFontSizeProperty, value); }
        }

        /// <summary>Identifies the <see cref="PriceTickFontSize">PriceTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty PriceTickFontSizeProperty =
            DependencyProperty.RegisterAttached("PriceTickFontSize", typeof(double), typeof(Chart),
                new FrameworkPropertyMetadata(Settings.DefaultPriceTickFontSize,
                    FrameworkPropertyMetadataOptions.Inherits, OnPriceTickFontSizeChanged));

        static void OnPriceTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Chart chart = obj as Chart;
            chart?.OnPropertyChanged("PriceAxisWidth");
            chart?.OnPropertyChanged("PriceTickTextHeight");
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets the width of the price and volume axis area.</summary>
        ///<value>The width of the price and volume axis area, which contains the ticks and its labels.</value>
        ///<remarks>
        /// The volume axis area has the same width as the price axis area.
        ///</remarks>
        public double PriceAxisWidth
        {
            get
            {
                double priceTextWidth = (new FormattedText(new string('A', MaxNumberOfCharsInPrice),
                    CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                    PriceTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(ChartControl).PixelsPerDip)).Width;
                return priceTextWidth + ValueTicksElement.TICK_LINE_WIDTH + ValueTicksElement.TICK_LEFT_MARGIN +
                       ValueTicksElement.TICK_RIGHT_MARGIN;
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets the height of the price or volume tick label.</summary>
        ///<value>The height of the price or volume tick label. This value is equals to the height of the text of the label.</value>
        ///<remarks>
        /// The volume tick label has the same height as the price tick label.
        ///</remarks>
        public double PriceTickTextHeight
        {
            get
            {
                return (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                    new Typeface("Verdana"), PriceTickFontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(ChartControl).PixelsPerDip)).Height;
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the minimal gap between the adjacent price or volume tick labels.</summary>
        ///<value>The minimal gap between adjacent labels for the price and volume axis. It must be non-negative value. The default is determined by the <see cref="DefaultGapBetweenPriceTickLabels"/> value.</value>
        ///<remarks>
        ///<para>This property regulates the density of the tick labels inside the price or volume axis area. The higher the <see cref="GapBetweenPriceTickLabels"/>, the less close adjacent labels are located.</para>
        ///<para>The volume axis doesn't have its own appearance properties. Therefore, the volume axis appearance depends on price axis properties.</para>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="GapBetweenPriceTickLabelsProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double GapBetweenPriceTickLabels
        {
            get { return (double)GetValue(GapBetweenPriceTickLabelsProperty); }
            set { SetValue(GapBetweenPriceTickLabelsProperty, value); }
        }

        /// <summary>Identifies the <see cref="GapBetweenPriceTickLabels">PriceTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty GapBetweenPriceTickLabelsProperty =
            DependencyProperty.RegisterAttached("GapBetweenPriceTickLabels", typeof(double), typeof(Chart),
                new FrameworkPropertyMetadata(Settings.DefaultGapBetweenPriceTickLabels,
                    FrameworkPropertyMetadataOptions.Inherits));


        //----------------------------------------------------------------------------------------------------------------------------------
        int maxNumberOfCharsInPrice = 0;

        /// <summary>Gets the maximal number of chars in a price for the current candle collection.</summary>
        ///<value>The maximal number of chars in a price for the current candle collection.</value>
        ///<remarks>
        ///This value is recalculated every time the candle collection source is changed, and remains constant until next change.
        ///</remarks>
        public int MaxNumberOfCharsInPrice
        {
            get { return maxNumberOfCharsInPrice; }
            private set
            {
                if (value == maxNumberOfCharsInPrice) return;
                maxNumberOfCharsInPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("PriceAxisWidth");
            }
        }

        // Просматривает CandlesSource и пересчитывает maxNumberOfCharsInPrice
        private void ReCalc_MaxNumberOfCharsInPrice()
        {
            if (CandlesSource == null || !CandlesSource.Any()) return;
            int charsInPrice = CandlesSource.Select(c => c.H.ToString().Length).Max();
            //int charsInVolume = IsVolumePanelVisible ? CandlesSource.Select(c => c.V.ToString().Length).Max() : 0;
            int charsInVolume = 0;
            MaxNumberOfCharsInPrice = Math.Max(charsInPrice, charsInVolume);
        }

        #endregion **********************************************************************************************************************************************

        #region PROPERTIES OF THE TIME AXIS

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the font size of the tick labels for the time axis.</summary>
        ///<value>The font size of the tick labels for the time (and date) axis. The default is determined by the <see cref="DefaultTimeTickFontSize"/> value.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="TimeTickFontSizeProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double TimeTickFontSize
        {
            get { return (double)GetValue(TimeTickFontSizeProperty); }
            set { SetValue(TimeTickFontSizeProperty, value); }
        }

        /// <summary>Identifies the <see cref="TimeTickFontSize">TimeTickFontSize</see> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty TimeTickFontSizeProperty =
            DependencyProperty.RegisterAttached("TimeTickFontSize", typeof(double), typeof(Chart),
                new FrameworkPropertyMetadata(Settings.DefaultTimeTickFontSize,
                    FrameworkPropertyMetadataOptions.Inherits, OnTimeTickFontSizeChanged));

        static void OnTimeTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Chart chart = obj as Chart;
            chart?.OnPropertyChanged("TimeAxisHeight");
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets the height of the time axis area.</summary>
        ///<value>The height of the time axis area, which contains the time and date ticks with its labels.</value>
        public double TimeAxisHeight
        {
            get
            {
                double timeTextHeight = (new FormattedText("1Ajl", CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface("Verdana"), TimeTickFontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(ChartControl).PixelsPerDip)).Height;
                return 2 * timeTextHeight + 4.0;
            }
        }

        #endregion **********************************************************************************************************************************************

        #region SCROLLBAR PROPERTIES

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the background for the scrollbar.</summary>
        ///<value>The brush for the scrollbar background. The default is determined by the <see cref="DefaultScrollBarBackground"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ScrollBarBackgroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush ScrollBarBackground
        {
            get { return (Brush)GetValue(ScrollBarBackgroundProperty); }
            set { SetValue(ScrollBarBackgroundProperty, value); }
        }

        /// <summary>Identifies the <see cref="ScrollBarBackground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ScrollBarBackgroundProperty =
            DependencyProperty.Register("ScrollBarBackground", typeof(Brush), typeof(Chart),
                new PropertyMetadata(DefaultScrollBarBackground));

        ///<summary>Gets the default value for the ScrollBarBackground property.</summary>
        ///<value>The default value for the <see cref="ScrollBarBackground"/> property: <c>#FFF0F0F0</c>.</value>
        public static Brush DefaultScrollBarBackground
        {
            get { return (Brush)(new SolidColorBrush(Color.FromArgb(255, 240, 240, 240))).GetCurrentValueAsFrozen(); }
        } // #FFF0F0F0

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the height of the scrollbar.</summary>
        ///<value>The height of the scrollbar background. The default is determined by the <see cref="DefaultScrollBarHeight"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ScrollBarHeightProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double ScrollBarHeight
        {
            get { return (double)GetValue(ScrollBarHeightProperty); }
            set { SetValue(ScrollBarHeightProperty, value); }
        }

        /// <summary>Identifies the <see cref="ScrollBarHeight"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ScrollBarHeightProperty =
            DependencyProperty.Register("ScrollBarHeight", typeof(double), typeof(Chart),
                new PropertyMetadata(DefaultScrollBarHeight));

        ///<summary>Gets the default value for the ScrollBarHeight property.</summary>
        ///<value>The default value for the <see cref="ScrollBarHeight"/> property: <c>15.0</c>.</value>
        public static double DefaultScrollBarHeight
        {
            get { return 15.0; }
        }

        #endregion **********************************************************************************************************************************************

        #region GRIDLINES PROPERTIES

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the pen for the horizontal gridlines.</summary>
        ///<value>The pen for the horizontal gridlines. The default is determined by the <see cref="DefaultHorizontalGridlinesBrush"/> and <see cref="DefaultHorizontalGridlinesThickness"/> values.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="HorizontalGridlinesPenProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Pen HorizontalGridlinesPen
        {
            get { return (Pen)GetValue(HorizontalGridlinesPenProperty); }
            set { SetValue(HorizontalGridlinesPenProperty, value); }
        }

        /// <summary>Identifies the <see cref="HorizontalGridlinesPen"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty HorizontalGridlinesPenProperty =
            DependencyProperty.Register("HorizontalGridlinesPen", typeof(Pen), typeof(Chart),
                new PropertyMetadata(DefaultHorizontalGridlinesPen));

        private static Pen DefaultHorizontalGridlinesPen
        {
            get
            {
                return (Pen)(new Pen(Settings.DefaultHorizontalGridlinesBrush,
                    Settings.DefaultHorizontalGridlinesThickness)).GetCurrentValueAsFrozen();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the pen for the vertical gridlines.</summary>
        ///<value>The pen for the vertical gridlines. The default is determined by the <see cref="DefaultVerticalGridlinesBrush"/> and <see cref="DefaultVerticalGridlinesThickness"/> values.</value>
        ///<remarks>
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VerticalGridlinesPenProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Pen VerticalGridlinesPen
        {
            get { return (Pen)GetValue(VerticalGridlinesPenProperty); }
            set { SetValue(VerticalGridlinesPenProperty, value); }
        }

        /// <summary>Identifies the <see cref="VerticalGridlinesPen"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VerticalGridlinesPenProperty =
            DependencyProperty.Register("VerticalGridlinesPen", typeof(Pen), typeof(Chart),
                new PropertyMetadata(DefaultVerticalGridlinesPen));


        private static Pen DefaultVerticalGridlinesPen
        {
            get
            {
                return (Pen)(new Pen(Settings.DefaultVerticalGridlinesBrush,
                    Settings.DefaultVerticalGridlinesThickness)).GetCurrentValueAsFrozen();
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the visibility of the horizontal gridlines.</summary>
        ///<value>The visibility of the horizontal gridlines: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsHorizontalGridlinesEnabled"/> values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsHorizontalGridlinesEnabledProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsVerticalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool IsHorizontalGridlinesEnabled
        {
            get { return (bool)GetValue(IsHorizontalGridlinesEnabledProperty); }
            set { SetValue(IsHorizontalGridlinesEnabledProperty, value); }
        }

        /// <summary>Identifies the <see cref="IsHorizontalGridlinesEnabled"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsHorizontalGridlinesEnabledProperty =
            DependencyProperty.Register("IsHorizontalGridlinesEnabled", typeof(bool), typeof(Chart),
                new PropertyMetadata(DefaultIsHorizontalGridlinesEnabled));

        ///<summary>Gets the default value for the IsHorizontalGridlinesEnabled property.</summary>
        ///<value>The default value for the <see cref="IsHorizontalGridlinesEnabled"/> property: <c>True</c>.</value>
        public static bool DefaultIsHorizontalGridlinesEnabled
        {
            get { return true; }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the vertical gridlines.</summary>
        ///<value>The visibility of the vertical gridlines: Visible if <c>True</c>; Hidden if <c>False</c>. The default is determined by the <see cref="DefaultIsVerticalGridlinesEnabled"/> values.</value>
        ///<remarks>
        ///This property applies to all vertical gridlines, which are showed for all ticks of the time axis. But sometimes you don't need to show all of this gridlines and want to visualize lines only for the most round time and date values. 
        ///In that case you need to set both the <see cref="IsVerticalGridlinesEnabled"/> and the <see cref="HideMinorVerticalGridlines"/> properties to <c>True</c>.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsVerticalGridlinesEnabledProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsHorizontalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool IsVerticalGridlinesEnabled
        {
            get { return (bool)GetValue(IsVerticalGridlinesEnabledProperty); }
            set { SetValue(IsVerticalGridlinesEnabledProperty, value); }
        }

        /// <summary>Identifies the <see cref="IsVerticalGridlinesEnabled"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsVerticalGridlinesEnabledProperty =
            DependencyProperty.Register("IsVerticalGridlinesEnabled", typeof(bool), typeof(Chart),
                new PropertyMetadata(DefaultIsVerticalGridlinesEnabled));

        ///<summary>Gets the default value for the IsVerticalGridlinesEnabled property.</summary>
        ///<value>The default value for the <see cref="IsVerticalGridlinesEnabled"/> property: <c>True</c>.</value>
        public static bool DefaultIsVerticalGridlinesEnabled
        {
            get { return true; }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility of the minor vertical gridlines.</summary>
        ///<value>The visibility of the vertical gridlines for minor (not "round") time ticks: Visible if <c>False</c>; Hidden if <c>True</c>. The default is determined by the <see cref="DefaultHideMinorVerticalGridlines"/>values.</value>
        ///<remarks>
        ///Sometimes you need to show gridlines only for the most round time or date values, and hide other minor gridlines.
        ///In that case you need to set both the <see cref="IsVerticalGridlinesEnabled"/> and the <see cref="HideMinorVerticalGridlines"/> properties to <c>True</c>.
        ///Whether the particular timetick value is Minor or not depends on the current timeframe. The common rule is: round time or date values are Major, others are Minor.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="HideMinorVerticalGridlinesProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        ///<seealso cref = "IsVerticalGridlinesEnabled">IsHorizontalGridlinesEnabled</seealso>
        public bool HideMinorVerticalGridlines
        {
            get { return (bool)GetValue(HideMinorVerticalGridlinesProperty); }
            set { SetValue(HideMinorVerticalGridlinesProperty, value); }
        }

        /// <summary>Identifies the <see cref="HideMinorVerticalGridlines"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty HideMinorVerticalGridlinesProperty =
            DependencyProperty.Register("HideMinorVerticalGridlines", typeof(bool), typeof(Chart),
                new PropertyMetadata(DefaultHideMinorVerticalGridlines));

        ///<summary>Gets the default value for the HideMinorVerticalGridlines property.</summary>
        ///<value>The default value for the <see cref="HideMinorVerticalGridlines"/> property: <c>False</c>.</value>
        public static bool DefaultHideMinorVerticalGridlines
        {
            get { return false; }
        }

        #endregion **********************************************************************************************************************************************

        DateTime lastCenterCandleDateTime;

        public bool IsLoaded { get; set; }


        private bool ReCalc_ValueHeight(int delta, int step = 1)
        {
            var visibleValuesExtremums = new ValuesExtremums(VisibleValuesExtremums.CandleValueHigh,
                VisibleValuesExtremums.CandleValueLow);

            foreach (var valueLow in VisibleValuesExtremums.ValuesLow)
            {
                visibleValuesExtremums.ValuesLow.Add(valueLow.Key, valueLow.Value);
            }

            foreach (var valueHigh in VisibleValuesExtremums.ValuesHigh)
            {
                visibleValuesExtremums.ValuesHigh.Add(valueHigh.Key, valueHigh.Value);
            }

            var tickSize = TickSize * 2;

            if (delta > 0)
            {
                visibleValuesExtremums.CandleValueHigh += tickSize;
                visibleValuesExtremums.CandleValueLow -= tickSize;
            }
            else
            {
                if (visibleValuesExtremums.CandleValueHigh - visibleValuesExtremums.CandleValueLow > 2 * tickSize)
                {
                    visibleValuesExtremums.CandleValueHigh -= tickSize;
                    visibleValuesExtremums.CandleValueLow += tickSize;
                }
            }

            VisibleValuesExtremums = visibleValuesExtremums;
            return true;
        }

        public ICommand GotoLiveCommand { get; set; }
        public ICommand FixedScaleCommand { get; set; }

        public ICommand DeleteDrawCommand { get; set; }




        public EventHandler<string> OrderCanceled;
        public EventHandler<OrderChangeEventArgs> OrderChanged;

        public Chart()
        {
            DrawingTools = new Draw();
            DrawingTools.SetChart(this);

            InitialCandleWidth = DefaultInitialCandleWidth;
            InitialCandleGap = DefaultInitialCandleGap;
            VisibleCandlesRange = IntRange.Undefined;
            VisibleValuesExtremums = new ValuesExtremums(0, 0);
            IsCrossLinesVisible = false;
            PanePlots = new Dictionary<PaneControl, ObservableCollection<Plot>>();


            GotoLiveCommand = new RelayCommand(GotoLive);
            FixedScaleCommand = new RelayCommand(SetFixedScale);

            DeleteDrawCommand = new RelayCommand(Delete);

            GotoLiveVisibility = Visibility.Collapsed;
            FixedScaleVisibility = Visibility.Collapsed;
        }

        private void Delete(object parameter)
        {
            if (parameter is BaseDraw baseDraw)
                SelectedDraw = baseDraw;
            DeleteDrawObject(SelectedDraw);
            SelectedDraw = DrawObjects.FirstOrDefault();
        }

        public void ClearDrawingObjects()
        {
            var baseDraws = LineDraws.Values.Where(x => x.IsDrawingObject).ToList();
            foreach (var baseDraw in baseDraws)
            {
                if (BarLines.ContainsKey(baseDraw.BarIndexStart))
                {
                    BarLines[baseDraw.BarIndexStart].Remove(baseDraw.Tag);

                    if (!BarLines[baseDraw.BarIndexStart].Any())
                    {
                        BarLines.Remove(baseDraw.BarIndexStart);
                    }
                }

                if (BarLines.ContainsKey(baseDraw.BarIndexEnd))
                {
                    BarLines[baseDraw.BarIndexEnd].Remove(baseDraw.Tag);

                    if (!BarLines[baseDraw.BarIndexEnd].Any())
                    {
                        BarLines.Remove(baseDraw.BarIndexEnd);
                    }
                }
                LineDraws.Remove(baseDraw.Tag);
            }

            foreach (var paneHLineDraw in PaneHLineDraws)
            {
                var lineDraws = paneHLineDraw.Value.Values.Where(x => x.IsDrawingObject).ToList();
                foreach (var lineDraw in lineDraws)
                {
                    paneHLineDraw.Value.Remove(lineDraw.Tag);
                }
            }



            var paneId = ChartControl.MainPane.Id;
            PaneTextDraws.TryGetValue(paneId, out var mainPaneTextDraws);
            DrawingTools.PaneTextDrawTags.TryGetValue(paneId, out var textDrawTags);
            var textDraws = TextDraws.Values.Where(x => x.IsDrawingObject).ToList();
            foreach (var textDraw in textDraws)
            {
                if (textDrawTags != null && textDrawTags.ContainsKey(textDraw.Tag))
                {
                    var drawInfo = textDrawTags[textDraw.Tag];
                    var idList = mainPaneTextDraws[drawInfo.BarIndex];
                    idList.Remove(drawInfo.Id);
                    textDrawTags.Remove(textDraw.Tag);
                }
                TextDraws.Remove(textDraw.Tag);
            }

            DrawObjects.Clear();
            RefreshMainPane();

        }

        public void DeleteDrawObject(BaseDraw draw)
        {
            if (draw is BaseLineDraw baseDraw)

            {
                if (BarLines.ContainsKey(baseDraw.BarIndexStart))
                {
                    BarLines[baseDraw.BarIndexStart].Remove(baseDraw.Tag);

                    if (!BarLines[baseDraw.BarIndexStart].Any())
                    {
                        BarLines.Remove(baseDraw.BarIndexStart);
                    }
                }

                if (BarLines.ContainsKey(baseDraw.BarIndexEnd))
                {
                    BarLines[baseDraw.BarIndexEnd].Remove(baseDraw.Tag);

                    if (!BarLines[baseDraw.BarIndexEnd].Any())
                    {
                        BarLines.Remove(baseDraw.BarIndexEnd);
                    }
                }
                LineDraws.Remove(baseDraw.Tag);

                foreach (var paneHLineDraw in PaneHLineDraws)
                {
                    var lineDraws = paneHLineDraw.Value.Values.Where(x => x.IsDrawingObject).ToList();
                    foreach (var lineDraw in lineDraws)
                    {
                        if (lineDraw.Tag == baseDraw.Tag)
                            paneHLineDraw.Value.Remove(lineDraw.Tag);
                    }
                }
            }



            if (draw is TextDraw textDraw)
            {
                var paneId = ChartControl.MainPane.Id;
                PaneTextDraws.TryGetValue(paneId, out var mainPaneTextDraws);
                DrawingTools.PaneTextDrawTags.TryGetValue(paneId, out var textDrawTags);

                if (textDrawTags != null && textDrawTags.ContainsKey(textDraw.Tag))
                {
                    var drawInfo = textDrawTags[textDraw.Tag];
                    var idList = mainPaneTextDraws[drawInfo.BarIndex];
                    idList.Remove(drawInfo.Id);
                    textDrawTags.Remove(textDraw.Tag);
                }
                TextDraws.Remove(textDraw.Tag);

            }

            DrawObjects.Remove(draw);
            RefreshMainPane();

        }

        private void SetFixedScale(object obj)
        {
            FixedScale = false;
            FixedScaleVisibility = Visibility.Collapsed;
            ReCalc_VisibleCandlesExtremums();
        }

        private void GotoLive(object obj)
        {
            VisibleCandlesRange =
                new IntRange(CandlesSource.Count - VisibleCandlesRange.Count, VisibleCandlesRange.Count);
        }


        // Заменили коллекцию CandlesSource на новую:
        static void OnCandlesSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var chart = obj as Chart;
            if (chart == null) return;

            if (e.OldValue != null)
            {
                ObservableCollection<ICandle> old_obsCollection = (ObservableCollection<ICandle>)e.OldValue;
                old_obsCollection.CollectionChanged -= chart.OnCandlesSourceCollectionChanged;
            }

            if (e.NewValue != null)
            {
                ObservableCollection<ICandle> new_obsCollection = (ObservableCollection<ICandle>)e.NewValue;
                new_obsCollection.CollectionChanged += chart.OnCandlesSourceCollectionChanged;
                BindingOperations.EnableCollectionSynchronization(new_obsCollection, chart.CandlesSourceLock);
            }

            if (chart.ChartControl != null && chart.ChartControl.IsLoaded)
            {
                chart.ReCalc_TimeFrame();
                chart.ReCalc_MaxNumberOfCharsInPrice();
                chart.ReCalc_MaxNumberOfDigitsAfterPointInPrice();

                if (chart.lastCenterCandleDateTime != DateTime.MinValue &&
                    chart.lastCenterCandleDateTime != DateTime.MaxValue)
                    chart.SetVisibleCandlesRangeCenter(chart.lastCenterCandleDateTime);
                else
                    chart.ReCalc_VisibleCandlesRange();

                //thisCandleChart.ReCalc_FinishedCandlesExtremums();
            }
        }


        // Произошли изменения содержимого коллекции CandlesSource:
        private void OnCandlesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //different kind of changes that may have occurred in collection
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex == (CandlesSource.Count - 1))
                {
                    //Добавляем свечу в конец
                    //if (CandlesSource.Count > 1)
                    //    ReCalc_FinishedCandlesExtremums_AfterNewFinishedCandleAdded(CandlesSource[CandlesSource.Count - 2]);

                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) == e.NewStartingIndex)
                    {
                        if (VisibleCandlesRange.Count < MaxVisibleRangeCandlesCount)
                        {
                            //Если видимый диапазон меньше максимально допустимого увеличиваем диапазон на 1
                            VisibleCandlesRange =
                                new IntRange(VisibleCandlesRange.Start_i, VisibleCandlesRange.Count + 1);
                        }
                        else
                        {
                            //Сдвигаем видимый диапазон на 1 единицу
                            VisibleCandlesRange =
                                new IntRange(VisibleCandlesRange.Start_i + 1, VisibleCandlesRange.Count);
                        }
                    }
                }
                else
                {
                    //Встаялем свечу
                    if (VisibleCandlesRange.Count < MaxVisibleRangeCandlesCount)
                    {
                        //Если видимый диапазон меньше максимально допустимого увеличиваем диапазон на 1
                        VisibleCandlesRange = new IntRange(VisibleCandlesRange.Start_i, VisibleCandlesRange.Count + 1);
                    }
                    else
                    {
                        //Сдвигаем видимый диапазон на 1 единицу
                        VisibleCandlesRange = new IntRange(VisibleCandlesRange.Start_i + 1, VisibleCandlesRange.Count);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                int vc_i = e.NewStartingIndex - VisibleCandlesRange.Start_i; // VisibleCandles index.
                if (vc_i >= 0 && vc_i < VisibleCandlesRange.Count)
                    ReCalc_VisibleCandlesExtremums_AfterOneCandleChanged(e.NewStartingIndex);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                /* your code */
            }

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                /* your code */
            }
        }


        // Просматривает CandlesSource и возвращает предполагаемый таймфрейм в минутах
        private void ReCalc_TimeFrame()
        {
            if (CandlesSource == null) return;

            Dictionary<TimeSpan, int> hist = new Dictionary<TimeSpan, int>();

            for (int i = 1; i < CandlesSource.Count; i++)
            {
                DateTime
                    t0 = CandlesSource[i - 1]
                        .t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i - 1].YYMMDD, CandlesSource[i - 1].HHMM);
                DateTime
                    t1 = CandlesSource[i]
                        .t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i].YYMMDD, CandlesSource[i].HHMM);
                TimeSpan ts = t1 - t0;
                if (hist.ContainsKey(ts))
                    hist[ts]++;
                else
                    hist.Add(ts, 1);
            }

            int max_freq = int.MinValue;
            TimeSpan max_freq_ts = TimeSpan.MinValue;
            foreach (KeyValuePair<TimeSpan, int> keyVal in hist)
            {
                if (keyVal.Value > max_freq)
                {
                    max_freq = keyVal.Value;
                    max_freq_ts = keyVal.Key;
                }
            }

            /*TimeFrame = (int) (max_freq_ts.TotalMinutes);*/
            TimeFrame = (int)(max_freq_ts.TotalSeconds);
        }


        // Просматривает CandlesSource и пересчитывает maxNumberOfCharsInPrice
        private void ReCalc_MaxNumberOfDigitsAfterPointInPrice()
        {
            if (CandlesSource == null) return;

            int max_n = 0;
            for (int i = 0; i < CandlesSource.Count; i++)
            {
                string str = CandlesSource[i].H.ToString();
                int point_i = str.LastIndexOf(',');
                if (point_i >= 0)
                {
                    int n = str.Length - point_i - 1;
                    if (n > max_n) max_n = n;
                }
            }

            MaxNumberOfDigitsAfterPointInPrice = max_n;
        }


        ///<summary>Shifts the range of visible candles to the position where the <c>t</c> property of the central visible candle is equal (or closest) to specified value.</summary>
        ///<param name="visibleCandlesRangeCenter">Central visible candle should have its <c>t</c> property equal to this parameter (or close to it as much as possible).</param>
        public void SetVisibleCandlesRangeCenter(DateTime visibleCandlesRangeCenter)
        {
            ICandle cndl = CandlesSource[VisibleCandlesRange.Count / 2];
            if (visibleCandlesRangeCenter < cndl.t) //MyDateAndTime.YYMMDDHHMM_to_Datetime(cndl.YYMMDD, cndl.HHMM))
            {
                VisibleCandlesRange = new IntRange(0, VisibleCandlesRange.Count);
                return;
            }

            cndl = CandlesSource[CandlesSource.Count - 1 - VisibleCandlesRange.Count / 2];
            if (visibleCandlesRangeCenter > cndl.t) // MyDateAndTime.YYMMDDHHMM_to_Datetime(cndl.YYMMDD, cndl.HHMM))
            {
                VisibleCandlesRange = new IntRange(CandlesSource.Count - VisibleCandlesRange.Count,
                    VisibleCandlesRange.Count);
                return;
            }

            VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(
                CandlesSource.FindCandleByDatetime(visibleCandlesRangeCenter) - VisibleCandlesRange.Count / 2);
        }


        // Пересчитывает VisibleCandlesRange.Count таким образом, чтобы по возможности сохранить индекс последней видимой свечи 
        // и соответствовать текущим значениям CandleWidth и CandleGap.
        public void ReCalc_VisibleCandlesRange()
        {
            var maxCount = MaxVisibleRangeCandlesCount;

            if (maxCount == 0 || CandlesSource == null)
            {
                VisibleCandlesRange = IntRange.Undefined;
                return;
            }

            int newCount = maxCount;
            if (newCount > CandlesSource.Count) newCount = CandlesSource.Count;
            int new_start_i = IntRange.IsUndefined(VisibleCandlesRange)
                ? (CandlesSource.Count - newCount)
                : VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - newCount;
            if (new_start_i < 0) new_start_i = 0;
            if (new_start_i + newCount > CandlesSource.Count)
                new_start_i = CandlesSource.Count - newCount;

            VisibleCandlesRange = new IntRange(new_start_i, newCount);
        }

        public void ReCalc_VisibleCandlesExtremums(double minL = double.MaxValue, double maxH = double.MinValue)
        {
            if (VisibleCandlesRange.Start_i < 0)
                return;

            var valuesExtremums = new ValuesExtremums(VisibleValuesExtremums.CandleValueHigh,
                VisibleValuesExtremums.CandleValueLow);

            if (CandlesSource != null)
            {
                int end_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - 1;

                //Calc Candle extremuns


                if (!FixedScale)
                {
                    if (minL == double.MaxValue && maxH == double.MinValue)
                    {
                        if (VisibleValuesExtremums.CandleValueHigh == 0 && VisibleValuesExtremums.CandleValueLow == 0)
                        {
                            for (int i = VisibleCandlesRange.Start_i; i <= end_i; i++)
                            {
                                if (CandlesSource.Count > i)
                                {
                                    ICandle cndl = CandlesSource[i];
                                    if (cndl.H > maxH) maxH = cndl.H;
                                    if (cndl.L < minL) minL = cndl.L;
                                }
                            }
                        }

                        if (minL > VisibleValuesExtremums.CandleValueLow)
                            minL = VisibleValuesExtremums.CandleValueLow;

                        if (maxH < VisibleValuesExtremums.CandleValueHigh)
                            maxH = VisibleValuesExtremums.CandleValueHigh;


                    }

                    valuesExtremums.CandleValueLow = minL;
                    valuesExtremums.CandleValueHigh = maxH;


                    /*if (((OrderDraws != null && OrderDraws.Any()) || PositionDraw != null) && IsChartTraderVisible)
                    {
                        double chartHeight = ChartControl.MainPane.valueTicksElement.ActualHeight - PriceChartTopMargin - PriceChartTopMargin;
                        var tickSizeHeight = chartHeight / ((maxH - minL) / TickSize);
                        var priceTickFontSize = 10.0;
                        double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                           FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                           VisualTreeHelper.GetDpi(ChartControl.MainPane.paneChart).PixelsPerDip)).Height;


                        var tickSizeCount = (int)((textHeight) / tickSizeHeight) + 2;

                        foreach (var orderDraw in OrderDraws.Values)
                        {
                            if (orderDraw.Price <= valuesExtremums.CandleValueLow)
                                valuesExtremums.CandleValueLow = (orderDraw.Price - TickSize * tickSizeCount);

                            if (orderDraw.Price >= valuesExtremums.CandleValueHigh)
                                valuesExtremums.CandleValueHigh = orderDraw.Price + TickSize * tickSizeCount;

                        }


                        if (PositionDraw != null)
                        {
                            if (PositionDraw.Price <= valuesExtremums.CandleValueLow)
                                valuesExtremums.CandleValueLow = (PositionDraw.Price - TickSize * tickSizeCount);

                            if (PositionDraw.Price >= valuesExtremums.CandleValueHigh)
                                valuesExtremums.CandleValueHigh = PositionDraw.Price + TickSize * tickSizeCount;
                        }

                    }*/

                    /*if ((BarLines != null && BarLines.Any()) && (LineDraws != null && LineDraws.Values.Any(x => x.IsAutoScale)))
                    {
                        double chartHeight = ChartControl.MainPane.valueTicksElement.ActualHeight - PriceChartTopMargin - PriceChartTopMargin;
                        var tickSizeHeight = chartHeight / ((maxH - minL) / TickSize);
                        var priceTickFontSize = 10.0;
                        double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                           FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                           VisualTreeHelper.GetDpi(ChartControl.MainPane.paneChart).PixelsPerDip)).Height;


                        var tickSizeCount = (int)((textHeight) / tickSizeHeight) + 2;

                        for (int i = 0; i < VisibleCandlesRange.Count; i++)
                        {
                            if (BarLines.TryGetValue(VisibleCandlesRange.Start_i + i, out var barLineList))
                            {
                                foreach (var lineTag in barLineList)
                                {
                                    var line = LineDraws[lineTag];
                                    if (line.IsAutoScale)
                                    {
                                        if (line.BarIndexEnd < VisibleCandlesRange.Start_i ||
                                            line.BarIndexStart > VisibleCandlesRange.Start_i + VisibleCandlesRange.Count)
                                            continue;

                                        if (line.StartY == -1 || line.EndY == -1)
                                            continue;

                                        if (line.StartY <= valuesExtremums.CandleValueLow)
                                            valuesExtremums.CandleValueLow = line.StartY - TickSize;

                                        if (line.StartY >= valuesExtremums.CandleValueHigh)
                                            valuesExtremums.CandleValueHigh = line.StartY + TickSize;

                                        if (line.EndY <= valuesExtremums.CandleValueLow)
                                            valuesExtremums.CandleValueLow = (line.EndY - TickSize);

                                        if (line.EndY >= valuesExtremums.CandleValueHigh)
                                            valuesExtremums.CandleValueHigh = line.EndY + TickSize;

                                    }

                                }
                            }
                        }
                    }*/

                    /*if (PlotExecutions > 0 && BarTrades.Any())
                    {
                        for (int i = 0; i < VisibleCandlesRange.Count; i++)
                        {



                            if (BarTrades.TryGetValue(VisibleCandlesRange.Start_i + i, out var barTradeList))
                            {
                                foreach (var tradeId in barTradeList)
                                {
                                    var tradeInfo = TradeDraws[tradeId];
                                    if (tradeInfo.EntryBar - 1 == VisibleCandlesRange.Start_i + i)
                                    {
                                        if (!tradeInfo.IsLong)
                                        {

                                        }
                                    }


                                }
                            }

                        }


                    }*/
                }
                else
                {
                    return;
                }
                //Calc PaneControl extremuns
                foreach (var panePlot in PanePlots)
                {
                    if (!panePlot.Key.IsMainPane || !FixedScale)
                    {
                        maxH = double.MinValue;
                        minL = double.MaxValue;
                        foreach (var plot in panePlot.Value)
                        {
                            if (plot.IsDataSourceEmpty || plot.Color.Equals(Colors.Transparent))
                                continue;

                            var series = plot.DataSource;

                            var startIndex = VisibleCandlesRange.Start_i;
                            var endIndex = VisibleCandlesRange.Count;
                            if (series.Count < end_i)
                            {
                                endIndex--;
                            }


                            maxH = Math.Max(maxH,
                                series.GetRange(startIndex, endIndex).DefaultIfEmpty(0).Max());
                            minL = Math.Min(minL,
                                series.GetRange(startIndex, endIndex).DefaultIfEmpty(0).Min());
                        }

                        valuesExtremums.ValuesLow.Add(panePlot.Key, minL);
                        valuesExtremums.ValuesHigh.Add(panePlot.Key, maxH);
                    }
                    else
                    {
                        valuesExtremums.ValuesLow.Add(panePlot.Key, VisibleValuesExtremums.ValuesLow[panePlot.Key]);
                        valuesExtremums.ValuesHigh.Add(panePlot.Key, VisibleValuesExtremums.ValuesHigh[panePlot.Key]);
                    }
                }
            }

            VisibleValuesExtremums = valuesExtremums;
        }


        private void ReCalc_VisibleCandlesExtremums_AfterOneCandleChanged(int changedCandle_i)
        {
            if (!FixedScale)
            {
                ICandle cndl = CandlesSource[changedCandle_i];
                double newPriceL = Math.Min(cndl.L, VisibleValuesExtremums.CandleValueLow);
                double newPriceH = Math.Max(cndl.H, VisibleValuesExtremums.CandleValueHigh);

                var visibleValuesExtremums = new ValuesExtremums(newPriceH, newPriceL)
                {
                    ValuesHigh = VisibleValuesExtremums.ValuesHigh,
                    ValuesLow = VisibleValuesExtremums.ValuesLow,
                    IsValuesCalculated = VisibleValuesExtremums.IsValuesCalculated
                };

                VisibleValuesExtremums = visibleValuesExtremums;
            }
            else
            {
                ChartControl.Invalidate();
            }
        }

        // Пересчитывает VisibleCandlesRange.Start_i, CandleWidth и CandleGap таким образом, 
        // чтобы установить заданное значение для VisibleCandlesRange.Count и по возможности сохраняет индекс последней видимой свечи. 
        internal void SetVisibleCandlesRangeCount(int newCount)
        {
            if (newCount > CandlesSource.Count) newCount = CandlesSource.Count;
            if (newCount == VisibleCandlesRange.Count) return;
            if (!ReCalc_CandleWidthAndGap(newCount)) return; // Если график уже нельзя больше сжимать.

            int new_start_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - newCount;
            if (new_start_i < 0) new_start_i = 0;

            //TODO:Check this
            //var actualWidth = ChartControl.MainPane.ActualWidth - PriceAxisWidth;
            //int maxCount = (int) (actualWidth / (CandleWidth + CandleGap));

            VisibleCandlesRange = new IntRange(new_start_i, newCount);
        }

        internal void SetNewValueHeight(int deltaSign, int step = 1)
        {
            if (!ReCalc_ValueHeight(deltaSign, step)) return;
        }


        public void OnLoad()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                //IsAlreadyLoaded = true;
                ReCalc_TimeFrame();
                ReCalc_MaxNumberOfCharsInPrice();
                ReCalc_MaxNumberOfDigitsAfterPointInPrice();

                CandleGap = InitialCandleGap;
                CandleWidth = InitialCandleWidth;
            }
        }


        public void OnMouseWheel(ModifierKeys modifiers, int delta)
        {
            if (modifiers == MouseWheelModifierKeyForCandleWidthChanging)
            {
                if (delta > 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count - 3);
                else if (delta < 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count + 3);
            }
            else if (modifiers == MouseWheelModifierKeyForScrollingThroughCandles)
            {
                if (delta > 0)
                {
                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) < CandlesSource.Count)
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i + 1);
                }
                else if (delta < 0)
                {
                    if (VisibleCandlesRange.Start_i > 0)
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i - 1);
                }
            }
        }


        public PaneControl AddPane(string paneName)
        {
            return ChartControl.AddPane(paneName);
        }

        public void RemovePane()
        {
            ChartControl.RemovePane();
        }

        public void RemoveAllPane()
        {
            ChartControl.RemoveAllPane();
        }

        public void AddSeries(ISeries<double> series)
        {
            if (Series == null)
                Series = new ObservableCollection<ISeries<double>>();

            Series.Add(series);
        }


        public void RemoveSeries(ISeries<double> series)
        {
            Series?.Remove(series);
        }

        public void AddPanePlot(PaneControl pane, Plot plot)
        {
            if (pane == null)
                pane = (PaneControl)ChartControl.FindName("MainPane");

            if (pane == null)
                throw new Exception("Main pane not found");

            if (PanePlots.TryGetValue(pane, out var plotsCollection))
            {
                plotsCollection.Add(plot);
            }
            else
            {
                PanePlots.Add(pane, new ObservableCollection<Plot>() { plot });
            }

            pane.RefreshPlots();
            ReCalc_VisibleCandlesExtremums();
        }

        public void RemovePlots(object owner)
        {
            foreach (var panePlot in PanePlots)
            {
                var plots = panePlot.Value.Where(x => x.Owner == owner).ToList();

                foreach (var plot in plots)
                {
                    plot.SetOwner(null);
                    panePlot.Value.Remove(plot);
                }
            }
        }


        public void RemovePanes(object owner)
        {
            var paneViews = PanePlots.Where(x => x.Key.Owner == owner).Select(x => x.Key).ToList();

            foreach (var paneView in paneViews)
            {
                paneView.SetOwner(null);
                PanePlots.Remove(paneView);
                ChartControl.RemovePane(paneView);
            }
        }


        public static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = LogicalTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }




        public void RemoveOrder(string orderGuid)
        {
            OrderDraws.Remove(orderGuid);
            ChartControl.MainPane.paneChart.RemoveOrder(orderGuid);
            ChartControl.MainPane.paneChart.Render();
            ChartControl.MainPane.valueTicksElement.Render();
        }

        public void RefreshMainPane()
        {
            if (ChartControl != null)
            {
                ChartControl.MainPane.paneChart.Render();
                ChartControl.MainPane.valueTicksElement.Render();
                ChartControl.MainPane.timeAxis.Render();
            }
        }

        public void RefreshPaneChart()
        {
            if (ChartControl != null)
            {
                ChartControl.MainPane.paneChart.Render();
            }
        }

        public void ShowOrderQntEdit(string key, Point point, double width, double height)
        {
            if (OrderDraws.TryGetValue(key, out var orderDraw))
            {
                Popup p = new Popup();
                p.Width = width * 2;
                p.Height = height + 2;
                p.PlacementTarget = ChartControl.MainPane.paneChart;
                p.Placement = PlacementMode.Relative;
                p.VerticalOffset = point.Y - 1;
                p.HorizontalOffset = point.X - width;
                p.StaysOpen = false;



                var tb = new TextBox();
                tb.Background = Brushes.WhiteSmoke;
                tb.Width = p.Width;
                tb.Height = p.Height;
                tb.FontSize = 12;
                tb.TextAlignment = TextAlignment.Right;
                tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.Text = orderDraw.Qnt.ToString();
                tb.Tag = orderDraw;
                tb.PreviewTextInput += (sender, e) =>
                {
                    if (e.Text.EndsWith(".") || e.Text.EndsWith(","))
                    {
                        e.Handled = false;
                        return;
                    }

                    Regex regex = new Regex("^[+-]?[0-9]{1,9}(?:\\.[0-9]{1,9})?$");
                    e.Handled = !regex.IsMatch(e.Text);
                };
                tb.KeyDown += (sender, e) =>
                {

                    if (e.Key == Key.Enter)
                    {

                        if (double.TryParse(tb.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var qnt))
                        {
                            if (orderDraw.Qnt != qnt && qnt != 0)
                            {
                                OrderChanged?.Invoke(this, new OrderChangeEventArgs(orderDraw.Guid, qnt, null));
                                p.IsOpen = false;
                            }
                        }
                    }
                };
                tb.Loaded += (sender, e) =>
                {
                    tb.Focusable = true;
                    tb.Focus();
                    tb.CaretIndex = tb.Text.Length;
                };

                p.Child = tb;
                p.IsOpen = true;
            }
        }

        public void ShowTextEdit(TextDraw textDraw, Point point, double width, double height)
        {
            //if (DrawObjects.TryGetValue(key, out var orderDraw))
            {
                Popup p = new Popup();
                
                p.Width = width * 2;
                p.Height = height + 2;
                p.PlacementTarget = ChartControl.MainPane.paneChart;
                p.Placement = PlacementMode.Relative;
                p.VerticalOffset = point.Y - 1;
                p.HorizontalOffset = point.X - 1;
                p.StaysOpen = false;
                p.AllowsTransparency = true;
                p.ClipToBounds = true;
                


                var tb = new TextBox();
                tb.Background = Brushes.WhiteSmoke;
                tb.BorderBrush = Brushes.Black;
                tb.Width = p.Width;
                tb.Height = p.Height;
                tb.FontFamily = new FontFamily("Verdana");
                tb.FontSize = textDraw.TextSize;
                tb.TextAlignment = TextAlignment.Left;
                tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalContentAlignment = VerticalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.Text = "";
                tb.Tag = textDraw;

                tb.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        textDraw.Text = tb.Text;
                        p.IsOpen = false;
                    }
                };

                tb.LostFocus += (sender, e) =>
                {
                    textDraw.Text = tb.Text;
                    p.IsOpen = false;
                };
                tb.Loaded += (sender, e) =>
                {
                    tb.Focusable = true;
                    tb.Focus();
                    tb.CaretIndex = tb.Text.Length;
                };

                p.Child = tb;
                p.IsOpen = true;
                RefreshPaneChart();
            }
        }

        

        public void CancelMoveOrderDraw()
        {
            ChartControl?.MainPane.paneChart.CancelMoveOrderDraw();
        }

        public void Reload()
        {
            if (ChartControl != null && ChartControl.IsLoaded)
            {
                ReCalc_VisibleCandlesRange();
                ReCalc_VisibleCandlesExtremums();
                ChartControl.Invalidate();
                Refresh();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void Refresh()
        {
            foreach (var panePlot in PanePlots)
            {
                panePlot.Key.RefreshPlots();
            }
        }

        public void Clear()
        {
            foreach (var panePlot in PanePlots)
            {
                panePlot.Value.Clear();
                panePlot.Key.RefreshPlots();

                if (panePlot.Key.IsMainPane)
                {
                    panePlot.Key.paneChart.Plots.Clear();
                    panePlot.Key.paneChart.Render();
                }
            }

            PanePlots.Clear();
            RemoveAllPane();

            if (Series != null)
            {
                foreach (var series in Series)
                {
                    series.Clear();
                }

                Series.Clear();
            }
        }

        public void Reset()
        {
            BarTrades?.Clear();
            TradeDraws?.Clear();
            ArrowDraws?.Clear();
            TextDraws?.Clear();
            BarColors?.Clear();
            LineDraws?.Clear();
            BarLines?.Clear();
            BarBackgroundsColors?.Clear();
            LineDraws?.Clear();
            BarLines?.Clear();
            PaneArrowDraws?.Clear();
            PaneTextDraws?.Clear();
            PaneHLineDraws?.Clear();
            OrderDraws?.Clear();
            PositionDraw = null;
        }

        private String crossLinesTime;

        public String CrossLinesTime
        {
            get => crossLinesTime;
            set
            {
                if (String.Equals(crossLinesTime, value, StringComparison.Ordinal))
                {
                    return;
                }

                crossLinesTime = value;
                OnPropertyChanged();
            }
        }

        private String _crossLinesOnlyTime;
        public String CrossLinesOnlyTime
        {
            get => _crossLinesOnlyTime;
            set
            {
                if (String.Equals(_crossLinesOnlyTime, value, StringComparison.Ordinal))
                {
                    return;
                }

                _crossLinesOnlyTime = value;
                OnPropertyChanged();
            }
        }

        private String crossLinesPrice;

        private Brush _markerTextColor;

        public String CrossLinesPrice
        {
            get => crossLinesPrice;
            set
            {
                if (String.Equals(crossLinesPrice, value, StringComparison.Ordinal))
                {
                    return;
                }

                crossLinesPrice = value;
                OnPropertyChanged();
            }
        }

        public DateTime? GetTimeFromPoint(Point point)
        {
            var index = (int)Math.Floor(point.X / (CandleWidth + CandleGap));

            if (index < 0 || index >= VisibleCandlesRange.Count)
            {
                return null;
            }

            index += VisibleCandlesRange.Start_i;
            if (index >= CandlesSource.Count)
                return null;
            return CandlesSource[index].t;
        }

        public int GetBarIndexFromPoint(Point point)
        {
            var index = (int)Math.Floor(point.X / (CandleWidth + CandleGap));

            if (index < 0 || index >= VisibleCandlesRange.Count)
            {
               // return -1;
            }

            index += VisibleCandlesRange.Start_i;
            /*if (index >= CandlesSource.Count)
                return -1;*/
            return index;
        }

        public Double? GetPriceFromPoint(Point mousePosition, PaneControl paneControl)
        {
            mousePosition = paneControl.TranslatePoint(mousePosition, paneControl.paneChart);
            var low = paneControl.IsMainPane
                ? VisibleValuesExtremums.CandleValueLow
                : VisibleValuesExtremums.GetValuesLow(paneControl);
            var high = paneControl.IsMainPane
                ? VisibleValuesExtremums.CandleValueHigh
                : VisibleValuesExtremums.GetValuesHigh(paneControl);

            return (1D - mousePosition.Y / paneControl.paneChart.RenderSize.Height) * (high - low) + low;
        }

        public String FormatTime(DateTime? dateTime)
        {
            return
                (dateTime?.ToString("d", CultureInfo.CurrentCulture) ?? String.Empty) + " " +
                (dateTime?.ToString("HH:mm:ss", CultureInfo.CurrentCulture) ?? String.Empty);
        }


        public String FormatDateTimeWithMilli(DateTime? dateTime)
        {
            return
                (dateTime?.ToString("d", CultureInfo.CurrentCulture) ?? String.Empty) + " " +
                (dateTime?.ToString("HH:mm:ss:fff", CultureInfo.CurrentCulture) ?? String.Empty);
        }
        public String FormatTimeWithMilli(DateTime? dateTime)
        {
            return (dateTime?.ToString("HH:mm:ss:fff", CultureInfo.CurrentCulture) ?? String.Empty);
        }

        public String FormatPrice(double? price) =>
            price?.ToString("F2", CultureInfo.CurrentCulture) ?? String.Empty;


        public void Destroy()
        {
            IsDestroying = true;
            Clear();
            Reset();
            BarTrades = null;
            TradeDraws = null;
            ArrowDraws = null;
            TextDraws = null;
            BarColors = null;
            LineDraws = null;
            BarLines = null;
            BarBackgroundsColors = null;
            LineDraws = null;
            BarLines = null;
            PaneArrowDraws = null;
            PaneTextDraws = null;
            PaneHLineDraws = null;
            OrderDraws = null;
            PositionDraw = null;
            ChartControl = null;
        }


        public void PriceLevelsScroll(int deltaY)
        {
            //PriceLevelScrolled?.Invoke(this, new PriceLevelScrollEventArgs(deltaY));
        }

        internal void SelectDraw(BaseDraw baseDraw)
        {
            if (SelectedDraw != null)
                SelectedDraw.IsSelected = false;
            baseDraw.IsSelected = true;
            SelectedDraw = baseDraw;
        }
    }
}