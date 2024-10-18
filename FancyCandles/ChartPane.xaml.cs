using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices; // [CallerMemberName]


namespace FancyCandles
{
    /// <summary>Represents the extreme values of Price and Volume for a set of candlesticks.</summary>
    
    /// <summary>Candlestick chart control derived from UserControl.</summary>
    public partial class ChartPane : UserControl, INotifyPropertyChanged
    {

        public static readonly double ToolTipFontSize = 9.0;


        void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            //IsAlreadyLoaded = true;
            ReCalc_TimeFrame();
            ReCalc_MaxNumberOfCharsInPrice();
        }


        /// <summary>
        /// ChartPane constructor
        /// </summary>
        public ChartPane()
        {
            InitializeComponent();

            VisibleCandlesRange = IntRange.Undefined;
            VisibleCandlesExtremums = new CandleExtremums(0.0, 0.0, 0L, 0L);
            Loaded += new RoutedEventHandler(OnUserControlLoaded);
        }
        
        /// <summary>Gets or sets the background of the price chart and volume diagram areas.</summary>
        ///<value>The background of the price chart and volume diagram areas. The default is determined by the <see cref="DefaultChartAreaBackground"/>values.</value>
        ///<remarks>
        ///This background is not applied to the horizontal and vertical axis areas, which contain tick marks and labels.
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="ChartAreaBackgroundProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush ChartAreaBackground
        {
            get { return (Brush)GetValue(ChartAreaBackgroundProperty); }
            set { SetValue(ChartAreaBackgroundProperty, value); }
        }

        /// <summary>Identifies the <see cref="ChartAreaBackground"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty ChartAreaBackgroundProperty =
            DependencyProperty.Register("ChartAreaBackground", typeof(Brush), typeof(ChartPane), new PropertyMetadata(DefaultChartAreaBackground));

        ///<summary>Gets the default value for the ChartAreaBackground property.</summary>
        ///<value>The default value for the <see cref="ChartAreaBackground"/> property: <c>#FFFFFDE9</c>.</value>
        public static Brush DefaultChartAreaBackground { get { return (Brush)(new SolidColorBrush(Color.FromArgb(255, 255, 253, 233))).GetCurrentValueAsFrozen(); } } // #FFFFFDE9
        //----------------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>Gets or sets the fill brush for the rectangle, that covers this chart control if it has been disabled.</summary>
        ///<value>The fill brush for the rectangle, that covers this chart control if it has been disabled. The default is determined by the <see cref="DefaultDisabledFill"/>values.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="DisabledFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush DisabledFill
        {
            get { return (Brush)GetValue(DisabledFillProperty); }
            set { SetValue(DisabledFillProperty, value); }
        }
        
        /// <summary>Identifies the <see cref="DisabledFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty DisabledFillProperty =
            DependencyProperty.Register("DisabledFill", typeof(Brush), typeof(ChartPane), new PropertyMetadata(DefaultDisabledFill));

        ///<summary>Gets the default value for the DisabledFill property.</summary>
        ///<value>The default value for the <see cref="DisabledFill"/> property: <c>#CCAAAAAA</c>.</value>
        public static Brush DefaultDisabledFill { get { return (Brush)(new SolidColorBrush(Color.FromArgb(204, 170, 170, 170))).GetCurrentValueAsFrozen(); } } // #CCAAAAAA
        //----------------------------------------------------------------------------------------------------------------------------------
        
        #region VOLUME HISTOGRAM PROPERTIES *********************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the visibility for the volume histogram panel.</summary>
        ///<value>The boolean value that means whether the volume histogram panel is visible or not. The default is determined by the <see cref="DefaultIsVolumePanelVisible"/> value.</value>
        ///<remarks> 
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="IsVolumePanelVisibleProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public bool IsVolumePanelVisible
        {
            get { return (bool)GetValue(IsVolumePanelVisibleProperty); }
            set { SetValue(IsVolumePanelVisibleProperty, value); }
        }
        /// <summary>Identifies the <see cref="IsVolumePanelVisible"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty IsVolumePanelVisibleProperty =
            DependencyProperty.Register("IsVolumePanelVisible", typeof(bool), typeof(ChartPane), new PropertyMetadata(DefaultIsVolumePanelVisible));

        ///<summary>Gets the default value for the IsVolumePanelVisible property.</summary>
        ///<value>The default value for the <see cref="IsVolumePanelVisible"/> property: <c>True</c>.</value>
        public static bool DefaultIsVolumePanelVisible { get { return true; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the volume bar width to the candle width ratio that eventually defines the width of the volume bar.</summary>
        ///<value>The ratio of the volume bar width to the candle width. The default is determined by the <see cref="DefaultVolumeBarWidthToCandleWidthRatio"/> value.</value>
        ///<remarks> 
        ///We define the width of the volume bar as a variable that is dependent on the candle width as follows:
        ///<p style="margin: 0 0 0 20"><em>Volume bar width</em> = <see cref="VolumeBarWidthToCandleWidthRatio"/> * <see cref="CandleWidth"/></p>
        ///The value of this property must be in the range [0, 1]. If the value of this property is zero then the volume bar width will be 1.0 in device-independent units, irrespective of the candle width.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeBarWidthToCandleWidthRatioProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeBarWidthToCandleWidthRatio
        {
            get { return (double)GetValue(VolumeBarWidthToCandleWidthRatioProperty); }
            set { SetValue(VolumeBarWidthToCandleWidthRatioProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeBarWidthToCandleWidthRatio"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeBarWidthToCandleWidthRatioProperty =
            DependencyProperty.Register("VolumeBarWidthToCandleWidthRatio", typeof(double), typeof(ChartPane), new PropertyMetadata(DefaultVolumeBarWidthToCandleWidthRatio, null, CoerceVolumeBarWidthToCandleWidthRatio));

        private static object CoerceVolumeBarWidthToCandleWidthRatio(DependencyObject objWithOldDP, object newDPValue)
        {
            //CandleChart thisCandleChart = (CandleChart)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            double newValue = (double)newDPValue;
            return Math.Min(1.0, Math.Max(0.0, newValue));
        }

        ///<summary>Gets the default value for the VolumeBarWidthToCandleWidthRatio property.</summary>
        ///<value>The default value for the <see cref="VolumeBarWidthToCandleWidthRatio"/> property: <c>0.3</c>.</value>
        ///<seealso cref = "VolumeBarWidthToCandleWidthRatio">VolumeBarWidthToCandleWidthRatio</seealso>
        public static double DefaultVolumeBarWidthToCandleWidthRatio { get { return 0.3; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets or sets the top margin for the volume histogram.</summary>
        ///<value>The top margin of the volume histogram, in device-independent units. The default is determined by the <see cref="DefaultVolumeHistogramTopMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the volume histogram inside its area by setting the <see cref="VolumeHistogramTopMargin"/> and <see cref="VolumeHistogramBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeHistogramTopMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeHistogramTopMargin
        {
            get { return (double)GetValue(VolumeHistogramTopMarginProperty); }
            set { SetValue(VolumeHistogramTopMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeHistogramTopMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeHistogramTopMarginProperty =
            DependencyProperty.Register("VolumeHistogramTopMargin", typeof(double), typeof(ChartPane), new PropertyMetadata(DefaultVolumeHistogramTopMargin));

        ///<summary>Gets the default value for VolumeHistogramTopMargin property.</summary>
        ///<value>The default value for the <see cref="VolumeHistogramTopMargin"/> property, in device-independent units: <c>10.0</c>.</value>
        public static double DefaultVolumeHistogramTopMargin { get { return 10.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the bottom margin for the volume histogram.</summary>
        ///<value>The bottom margin of the volume histogram, in device-independent units. The default is determined by the <see cref="DefaultVolumeHistogramBottomMargin"/> value.</value>
        ///<remarks> 
        ///You can set up top and bottom margins for the volume histogram inside its area by setting the <see cref="VolumeHistogramTopMargin"/> and <see cref="VolumeHistogramBottomMargin"/> properties respectively.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="VolumeHistogramBottomMarginProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public double VolumeHistogramBottomMargin
        {
            get { return (double)GetValue(VolumeHistogramBottomMarginProperty); }
            set { SetValue(VolumeHistogramBottomMarginProperty, value); }
        }
        /// <summary>Identifies the <see cref="VolumeHistogramBottomMargin"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty VolumeHistogramBottomMarginProperty =
            DependencyProperty.Register("VolumeHistogramBottomMargin", typeof(double), typeof(ChartPane), new PropertyMetadata(DefaultVolumeHistogramBottomMargin));

        ///<summary>Gets the default value for VolumeHistogramBottomMargin property.</summary>
        ///<value>The default value for the <see cref="VolumeHistogramBottomMargin"/> property, in device-independent units: <c>5.0</c>.</value>
        public static double DefaultVolumeHistogramBottomMargin { get { return 5.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bullish volume bar.</summary>
        ///<value>The brush to fill all bullish volume bars. The default is determined by the <see cref="DefaultBullishVolumeBarFill"/> value.</value>
        ///<remarks> 
        /// We separate all volume bars to "bullish" or "bearish" according to whether the correspondent candle is bullish or bearish. A candle is bullish if its Close higher than its Open. A candle is Bearish if its Close lower than its Open. To visualize such a separation all bars are painted into two different colors - 
        /// <see cref="BullishVolumeBarFill"/> and <see cref="BearishVolumeBarFill"/> for bullish and bearish bars respectively. Likewise you can set the <see cref="BullishCandleFill"/> and <see cref="BearishCandleFill"/> properties to change the appearance of bullish and bearish price candles.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BullishVolumeBarFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BullishVolumeBarFill
        {
            get { return (Brush)GetValue(BullishVolumeBarFillProperty); }
            set { SetValue(BullishVolumeBarFillProperty, value); }
        }
        /// <summary>Identifies the <see cref="BullishVolumeBarFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BullishVolumeBarFillProperty =
            DependencyProperty.Register("BullishVolumeBarFill", typeof(Brush), typeof(ChartPane), new PropertyMetadata(DefaultBullishVolumeBarFill));

        ///<summary>Gets the default value for the BullishVolumeBarFill property.</summary>
        ///<value>The default value for the BullishVolumeBarFill property: <c>Brushes.Green</c>.</value>
        ///<seealso cref = "BullishVolumeBarFill">BullishVolumeBarFill</seealso>
        public static Brush DefaultBullishVolumeBarFill { get { return (Brush)(new SolidColorBrush(Colors.Green)).GetCurrentValueAsFrozen(); } }
        //----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the color of the bearish volume bar.</summary>
        ///<value>The brush to fill all bearish volume bars. The default is determined by the <see cref="DefaultBearishVolumeBarFill"/> value.</value>
        ///<remarks> 
        /// We separate all volume bars to "bullish" or "bearish" according to whether the correspondent candle is bullish or bearish. The Bullish candle has its Close higher than its Open. The Bearish candle has its Close lower than its Open. To visualize such a separation all bars are painted into two different colors - 
        /// <see cref="BullishVolumeBarFill"/> and <see cref="BearishVolumeBarFill"/> for bullish and bearish bars respectively. Likewise you can set the <see cref="BullishCandleFill"/> and <see cref="BearishCandleFill"/> properties to change the appearance of bullish and bearish price candles.
        ///<h3>Dependency Property Information</h3>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="BearishVolumeBarFillProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public Brush BearishVolumeBarFill
        {
            get { return (Brush)GetValue(BearishVolumeBarFillProperty); }
            set { SetValue(BearishVolumeBarFillProperty, value); }
        }
        /// <summary>Identifies the <see cref="BearishVolumeBarFill"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty BearishVolumeBarFillProperty =
            DependencyProperty.Register("BearishVolumeBarFill", typeof(Brush), typeof(ChartPane), new PropertyMetadata(DefaultBearishVolumeBarFill));

        ///<summary>Gets the default value for the BearishVolumeBarFill property.</summary>
        ///<value>The default value for the BearishVolumeBarFill property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishVolumeBarFill">BearishVolumeBarFill</seealso>
        public static Brush DefaultBearishVolumeBarFill { get { return (Brush)(new SolidColorBrush(Colors.Red)).GetCurrentValueAsFrozen(); } }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        
        
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
            DependencyProperty.RegisterAttached("PriceTickFontSize", typeof(double), typeof(ChartPane), new FrameworkPropertyMetadata(DefaultPriceTickFontSize, FrameworkPropertyMetadataOptions.Inherits, OnPriceTickFontSizeChanged));
        static void OnPriceTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChartPane thisCandleChart = obj as ChartPane;
            thisCandleChart?.OnPropertyChanged("PriceAxisWidth");
            thisCandleChart?.OnPropertyChanged("PriceTickTextHeight");
        }

        ///<summary>Gets the default value for the <see cref="PriceTickFontSize">PriceTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="PriceTickFontSize"/> property: <c>11.0</c>.</value>
        public static double DefaultPriceTickFontSize { get { return 11.0; } }
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
                double priceTextWidth = (new FormattedText(new string('A', MaxNumberOfCharsInPrice), CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), PriceTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Width;
                return priceTextWidth + PriceTicksElement.TICK_LINE_WIDTH + PriceTicksElement.TICK_LEFT_MARGIN + PriceTicksElement.TICK_RIGHT_MARGIN;
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
                return (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), PriceTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
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
            DependencyProperty.RegisterAttached("GapBetweenPriceTickLabels", typeof(double), typeof(ChartPane), new FrameworkPropertyMetadata(DefaultGapBetweenPriceTickLabels, FrameworkPropertyMetadataOptions.Inherits));
        
        ///<summary>Gets the default value for the <see cref="GapBetweenPriceTickLabels">GapBetweenPriceTickLabels</see> property.</summary>
        ///<value>The default value for the <see cref="GapBetweenPriceTickLabels"/> property: <c>0.0</c>.</value>
        public static double DefaultGapBetweenPriceTickLabels { get { return 0.0; } }
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
        void ReCalc_MaxNumberOfCharsInPrice()
        {
            if (CandlesSource == null) return;
            int charsInPrice = CandlesSource.Select(c => c.H.ToString().Length).Max();
            int charsInVolume = IsVolumePanelVisible ? CandlesSource.Select(c => c.V.ToString().Length).Max() : 0;
            MaxNumberOfCharsInPrice = Math.Max(charsInPrice, charsInVolume);
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        #region PROPERTIES OF THE TIME AXIS *********************************************************************************************************************
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
            DependencyProperty.RegisterAttached("TimeTickFontSize", typeof(double), typeof(ChartPane), new FrameworkPropertyMetadata(DefaultTimeTickFontSize, FrameworkPropertyMetadataOptions.Inherits, OnTimeTickFontSizeChanged));
        static void OnTimeTickFontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChartPane thisCandleChart = obj as ChartPane;
            thisCandleChart?.OnPropertyChanged("TimeAxisHeight");
        }

        ///<summary>Gets the default value for the <see cref="TimeTickFontSize">TimeTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="TimeTickFontSize"/> property: <c>10.0</c>.</value>
        public static double DefaultTimeTickFontSize { get { return 10.0; } }
        //----------------------------------------------------------------------------------------------------------------------------------
        ///<summary>Gets the height of the time axis area.</summary>
        ///<value>The height of the time axis area, which contains the time and date ticks with its labels.</value>
        public double TimeAxisHeight
        {
            get
            {
                double timeTextHeight = (new FormattedText("1Ajl", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), TimeTickFontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                return 2 * timeTextHeight + 4.0;
            }
        }

        #endregion **********************************************************************************************************************************************
        //----------------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>Gets or sets the data source for the candles of this chart.</summary>
        ///<value>The data source for the candles of this chart. The default value is null.</value>
        ///<remarks>
        ///<table border="1" frame="hsides" rules="rows" style="margin: 0 0 10 20"> 
        ///<tr><td>Identifier field</td><td><see cref="CandlesSourceProperty"/></td></tr> 
        ///<tr><td>Metadata properties set to <c>True</c></td><td>-</td></tr> </table>
        ///</remarks>
        public ObservableCollection<ICandle> CandlesSource
        {
            get { return (ObservableCollection<ICandle>)GetValue(CandlesSourceProperty); }
            set { SetValue(CandlesSourceProperty, value); }
        }
        /// <summary>Identifies the <see cref="CandlesSource"/> dependency property.</summary>
        /// <value><see cref="DependencyProperty"/></value>
        public static readonly DependencyProperty CandlesSourceProperty =
            DependencyProperty.Register("CandlesSource", typeof(ObservableCollection<ICandle>), typeof(ChartPane), new UIPropertyMetadata(null, OnCandlesSourceChanged, CoerceCandlesSource));

        DateTime lastCenterCandleDateTime;
        private static object CoerceCandlesSource(DependencyObject objWithOldDP, object newDPValue)
        {
            ChartPane thisCandleChart = (ChartPane)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            ObservableCollection<ICandle> newValue = (ObservableCollection<ICandle>)newDPValue;

            IntRange vcRange = thisCandleChart.VisibleCandlesRange;
            if (IntRange.IsUndefined(vcRange))
                thisCandleChart.lastCenterCandleDateTime = DateTime.MinValue;
            else
            {
                if (thisCandleChart.CandlesSource != null && (vcRange.Start_i + vcRange.Count) < thisCandleChart.CandlesSource.Count)
                {
                    int centralCandle_i = (2 * vcRange.Start_i + vcRange.Count) / 2;
                    thisCandleChart.lastCenterCandleDateTime = thisCandleChart.CandlesSource[centralCandle_i].t;
                }
                else
                    thisCandleChart.lastCenterCandleDateTime = DateTime.MaxValue;
            }

            return newValue;
        }

        // Заменили коллекцию CandlesSource на новую:
        static void OnCandlesSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChartPane thisCandleChart = obj as ChartPane;
            if (thisCandleChart == null) return;

            if (e.OldValue != null)
            {
                ObservableCollection<ICandle> old_obsCollection = (ObservableCollection<ICandle>)e.OldValue;
                old_obsCollection.CollectionChanged -= thisCandleChart.OnCandlesSourceCollectionChanged;
            }

            if (e.NewValue != null)
            {
                ObservableCollection<ICandle> new_obsCollection = (ObservableCollection<ICandle>)e.NewValue;
                new_obsCollection.CollectionChanged += thisCandleChart.OnCandlesSourceCollectionChanged;
            }

            if (thisCandleChart.IsLoaded)
            {
                thisCandleChart.ReCalc_TimeFrame();
                thisCandleChart.ReCalc_MaxNumberOfCharsInPrice();
                

                /*if (thisCandleChart.lastCenterCandleDateTime != DateTime.MinValue)
                    thisCandleChart.SetVisibleCandlesRangeCenter(thisCandleChart.lastCenterCandleDateTime);
                else
                    thisCandleChart.ReCalc_VisibleCandlesRange();*/

                //thisCandleChart.ReCalc_FinishedCandlesExtremums();
                thisCandleChart.ReCalc_VisibleCandlesExtremums();
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
                    //if (CandlesSource.Count > 1)
                    //    ReCalc_FinishedCandlesExtremums_AfterNewFinishedCandleAdded(CandlesSource[CandlesSource.Count - 2]);

                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) == e.NewStartingIndex)
                        VisibleCandlesRange = new IntRange(VisibleCandlesRange.Start_i + 1, VisibleCandlesRange.Count);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                int vc_i = e.NewStartingIndex - VisibleCandlesRange.Start_i; // VisibleCandles index.
                if (vc_i >= 0 && vc_i < VisibleCandlesRange.Count)
                    ReCalc_VisibleCandlesExtremums_AfterOneCandleChanged(e.NewStartingIndex);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove) { /* your code */ }
            if (e.Action == NotifyCollectionChangedAction.Move) { /* your code */ }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        int timeFrame;
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

        // Просматривает CandlesSource и возвращает предполагаемый таймфрейм в минутах
        void ReCalc_TimeFrame()
        {
            if (CandlesSource == null) return;

            Dictionary<TimeSpan, int> hist = new Dictionary<TimeSpan, int>();

            for (int i = 1; i < CandlesSource.Count; i++)
            {
                DateTime t0 = CandlesSource[i - 1].t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i - 1].YYMMDD, CandlesSource[i - 1].HHMM);
                DateTime t1 = CandlesSource[i].t; // MyDateAndTime.YYMMDDHHMM_to_Datetime(CandlesSource[i].YYMMDD, CandlesSource[i].HHMM);
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

            TimeFrame = (int)(max_freq_ts.TotalMinutes);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        CandleExtremums visibleCandlesExtremums;
        ///<summary>Gets the Low and High of the visible candles in vector format (Low,High).</summary>
        ///<value>The Low and High of the visible candles in vector format (Low,High).</value>
        ///<remarks>
        ///<para>The visible candles are those that fall inside the visible candles range, which is determined by the <see cref="VisibleCandlesRange"/> property.</para>
        ///The Low of a set of candles is a minimum Low value of this candles. The High of a set of candles is a maximum High value of this candles.
        ///</remarks>
        public CandleExtremums VisibleCandlesExtremums
        {
            get { return visibleCandlesExtremums; }
            private set
            {
                visibleCandlesExtremums = value;
                OnPropertyChanged();
            }
        }

        void ReCalc_VisibleCandlesExtremums()
        {
            int end_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - 1;
            double maxH = double.MinValue;
            double minL = double.MaxValue;
            long maxV = long.MinValue;
            long minV = long.MaxValue;
            for (int i = VisibleCandlesRange.Start_i; i <= end_i; i++)
            {
                ICandle cndl = CandlesSource[i];
                if (cndl.H > maxH) maxH = cndl.H;
                if (cndl.L < minL) minL = cndl.L;
                if (cndl.V < minV) minV = cndl.V;
                if (cndl.V > maxV) maxV = cndl.V;
            }

            VisibleCandlesExtremums = new CandleExtremums(minL, maxH, minV, maxV);
        }

        void ReCalc_VisibleCandlesExtremums_AfterOneCandleChanged(int changedCandle_i)
        {
            ICandle cndl = CandlesSource[changedCandle_i];
            double newPriceL = Math.Min(cndl.L, VisibleCandlesExtremums.PriceLow);
            double newPriceH = Math.Max(cndl.H, VisibleCandlesExtremums.PriceHigh);
            long newVolL = Math.Min(cndl.V, VisibleCandlesExtremums.VolumeLow);
            long newVolH = Math.Max(cndl.V, VisibleCandlesExtremums.VolumeHigh);
            VisibleCandlesExtremums = new CandleExtremums(newPriceL, newPriceH, newVolL, newVolH);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
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
            DependencyProperty.Register("VisibleCandlesRange", typeof(IntRange), typeof(ChartPane), new PropertyMetadata(IntRange.Undefined, OnVisibleCanlesRangeChanged, CoerceVisibleCandlesRange));

        static void OnVisibleCanlesRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChartPane thisCandleChart = (ChartPane)obj;
            if (thisCandleChart.IsLoaded)
                thisCandleChart.ReCalc_VisibleCandlesExtremums();
        }

        private static object CoerceVisibleCandlesRange(DependencyObject objWithOldDP, object baseValue)
        {
            ChartPane thisCandleChart = (ChartPane)objWithOldDP; // Содержит старое значение для изменяемого свойства.
            IntRange newValue = (IntRange)baseValue;

            if (IntRange.IsUndefined(newValue))
                return newValue;
            // Это хак для привязки к скроллеру, когда передается только компонента IntRange.Start_i, а компонента IntRange.Count берется из старого значения свойства:
            else if (IntRange.IsContainsOnlyStart_i(newValue))
                return new IntRange(newValue.Start_i, thisCandleChart.VisibleCandlesRange.Count);
            // А это обычная ситуация:
            else
            {
                int newVisibleCandlesStart_i = Math.Max(0, newValue.Start_i);
                int newVisibleCandlesEnd_i = Math.Min(thisCandleChart.CandlesSource.Count - 1, newValue.Start_i + Math.Max(1, newValue.Count) - 1);
                int newVisibleCandlesCount = newVisibleCandlesEnd_i - newVisibleCandlesStart_i + 1;
                /*if (newVisibleCandlesCount > maxVisibleCandlesCount)
                {
                    newVisibleCandlesStart_i = newVisibleCandlesEnd_i - maxVisibleCandlesCount + 1;
                    newVisibleCandlesCount = maxVisibleCandlesCount;
                }*/

                return new IntRange(newVisibleCandlesStart_i, newVisibleCandlesCount);
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        
        //----------------------------------------------------------------------------------------------------------------------------------
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
                VisibleCandlesRange = new IntRange(CandlesSource.Count - VisibleCandlesRange.Count, VisibleCandlesRange.Count);
                return;
            }

            VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(CandlesSource.FindCandleByDatetime(visibleCandlesRangeCenter) - VisibleCandlesRange.Count / 2);
        }

        ///<summary>Sets the range of visible candles, that starts and ends at specified moments in time.</summary>
        ///<param name="lowerBound">The datetime value at which the range of visible candles must start.</param>
        ///<param name="upperBound">The datetime value at which the range of visible candles must end.</param>
        ///<remarks>
        ///This function finds in the <see cref="CandlesSource"/> collection of candles two of them that has its <c>t</c> property equal or closest to <c>datetime0</c> and <c>datetime1</c>. 
        ///Then it sets the <see cref="VisibleCandlesRange"/> to the <see cref="IntRange"/> value that starts at the index of the first aforementioned candle, and ends at the index of the second one.
        ///</remarks>
        public void SetVisibleCandlesRangeBounds(DateTime lowerBound, DateTime upperBound)
        {
            if (CandlesSource == null || CandlesSource.Count == 0) return;

            if (lowerBound > upperBound)
            {
                DateTime t_ = lowerBound;
                lowerBound = upperBound;
                upperBound = t_;
            }

            int i0, i1;
            int N = CandlesSource.Count;
            if (CandlesSource[0].t > upperBound)
            {
                VisibleCandlesRange = new IntRange(0, 1);
                return;
            }

            if (CandlesSource[N - 1].t < lowerBound)
            {
                VisibleCandlesRange = new IntRange(N - 1, 1);
                return;
            }

            if (CandlesSource[0].t > lowerBound)
                i0 = 0;
            else
                i0 = CandlesSource.FindCandleByDatetime(lowerBound);

            if (CandlesSource[N - 1].t < upperBound)
                i1 = N - 1;
            else
                i1 = CandlesSource.FindCandleByDatetime(upperBound);

            int newVisibleCandlesCount = i1 - i0 + 1;
            VisibleCandlesRange = new IntRange(i0, newVisibleCandlesCount);
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
            DependencyProperty.Register("MouseWheelModifierKeyForCandleWidthChanging", typeof(ModifierKeys), typeof(ChartPane), new PropertyMetadata(ModifierKeys.None));
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
            DependencyProperty.Register("MouseWheelModifierKeyForScrollingThroughCandles", typeof(ModifierKeys), typeof(ChartPane), new PropertyMetadata(ModifierKeys.Control));
        //--------
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Пересчитывает VisibleCandlesRange.Start_i, CandleWidth и CandleGap таким образом, 
            // чтобы установить заданное значение для VisibleCandlesRange.Count и по возможности сохраняет индекс последней видимой свечи. 
            void SetVisibleCandlesRangeCount(int newCount)
            {
                if (newCount > CandlesSource.Count) newCount = CandlesSource.Count;
                if (newCount == VisibleCandlesRange.Count) return;


                int new_start_i = VisibleCandlesRange.Start_i + VisibleCandlesRange.Count - newCount;
                if (new_start_i < 0) new_start_i = 0;
                VisibleCandlesRange = new IntRange(new_start_i, newCount);
            }
            //------

            if (Keyboard.Modifiers == MouseWheelModifierKeyForCandleWidthChanging)
            {
                if (e.Delta > 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count - 3);
                else if (e.Delta < 0)
                    SetVisibleCandlesRangeCount(VisibleCandlesRange.Count + 3);
            }
            else if (Keyboard.Modifiers == MouseWheelModifierKeyForScrollingThroughCandles)
            {
                if (e.Delta > 0)
                {
                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) < CandlesSource.Count)
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i + 1);
                }
                else if (e.Delta < 0)
                {
                    if (VisibleCandlesRange.Start_i > 0) 
                        VisibleCandlesRange = IntRange.CreateContainingOnlyStart_i(VisibleCandlesRange.Start_i - 1);
                }
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------

        private void OnMouseMoveInsideVolumeHistogramContainer(object sender, MouseEventArgs e)
        {
            CurrentMousePosition = Mouse.GetPosition(volumeHistogramContainer);
        }

        Point currentMousePosition;
        /// <summary>This is a property for internal use only. You should not use it.</summary>
        public Point CurrentMousePosition
        {
            get { return currentMousePosition; }
            private set
            {
                if (currentMousePosition == value) return;
                currentMousePosition = value;
                OnPropertyChanged();
            }
        }

        //---------------- INotifyPropertyChanged ----------------------------------------------------------
        /// <summary>INotifyPropertyChanged interface realization.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>INotifyPropertyChanged interface realization.</summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
    }
}