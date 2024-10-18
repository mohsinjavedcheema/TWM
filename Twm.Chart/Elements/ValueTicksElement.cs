using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Twm.Chart.Classes;
using Twm.Chart.Controls;

namespace Twm.Chart.Elements
{
    class ValueTicksElement : FrameworkElement
    {
        public Classes.Chart Chart { get; set; }
        public static double TICK_LINE_WIDTH = 3.0;
        public static double TICK_LEFT_MARGIN = 2.0;
        public static double TICK_RIGHT_MARGIN = 1.0;

        //---------------------------------------------------------------------------------------------------------------------------------------
        static ValueTicksElement()
        {
            Pen defaultPen = new Pen(Settings.DefaultHorizontalGridlinesBrush,
                Settings.DefaultHorizontalGridlinesThickness);
            defaultPen.Freeze();
            GridlinesPenProperty = DependencyProperty.Register("GridlinesPen", typeof(Pen), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(defaultPen, null, CoerceGridlinesPen) { AffectsRender = true });
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public ValueTicksElement()
        {
            if (_axisTickPen == null)
            {
                _axisTickPen = new Pen(Settings.DefaultAxisTickColor, 1.0);
                if (!_axisTickPen.IsFrozen)
                    _axisTickPen.Freeze();
            }

            DataContextChanged += ValueTicksElement_DataContextChanged;
            SizeChanged += ValueTicksElement_SizeChanged;
        }

        private void ValueTicksElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Chart?.ChartControl.OnSizeChanged();
        }

        private void ValueTicksElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is Classes.Chart chart)
            {
                Chart = chart;
            }
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(ValueTicksElement),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty PaneWidthProperty = DependencyProperty.Register(
            "PaneWidth", typeof(double), typeof(ValueTicksElement),
            new FrameworkPropertyMetadata(0.0d, null, null) { AffectsRender = true });

        public double PaneWidth
        {
            get { return (double)GetValue(PaneWidthProperty); }
            set { SetValue(PaneWidthProperty, value); }
        }

        public static readonly DependencyProperty IsMainPaneProperty
            = DependencyProperty.Register("IsMainPane", typeof(bool), typeof(ValueTicksElement),
                new UIPropertyMetadata(null));

        public bool IsMainPane
        {
            get { return (bool)GetValue(IsMainPaneProperty); }
            set { SetValue(IsMainPaneProperty, value); }
        }


        public static readonly DependencyProperty PaneControlProperty
            = DependencyProperty.Register("PaneControl", typeof(PaneControl), typeof(ValueTicksElement),
                new UIPropertyMetadata(null));

        public PaneControl PaneControl
        {
            get { return (PaneControl)GetValue(PaneControlProperty); }
            set { SetValue(PaneControlProperty, value); }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public Pen GridlinesPen
        {
            get { return (Pen)GetValue(GridlinesPenProperty); }
            set { SetValue(GridlinesPenProperty, value); }
        }

        public static readonly DependencyProperty GridlinesPenProperty;

        private static object CoerceGridlinesPen(DependencyObject objWithOldDP, object newDPValue)
        {
            Pen newPenValue = (Pen)newDPValue;
            return newPenValue.IsFrozen ? newDPValue : newPenValue.GetCurrentValueAsFrozen();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public bool IsGridlinesEnabled
        {
            get { return (bool)GetValue(IsGridlinesEnabledProperty); }
            set { SetValue(IsGridlinesEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsGridlinesEnabledProperty
            = DependencyProperty.Register("IsGridlinesEnabled", typeof(bool), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(true) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        public ValuesExtremums VisibleValuesExtremums
        {
            get { return (ValuesExtremums)GetValue(VisibleValuesExtremumsProperty); }
            set { SetValue(VisibleValuesExtremumsProperty, value); }
        }

        public static readonly DependencyProperty VisibleValuesExtremumsProperty
            = DependencyProperty.Register("VisibleValuesExtremums", typeof(ValuesExtremums), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(new ValuesExtremums(0, 0)) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        public double GapBetweenTickLabels
        {
            get { return (double)GetValue(GapBetweenTickLabelsProperty); }
            set { SetValue(GapBetweenTickLabelsProperty, value); }
        }

        public static readonly DependencyProperty GapBetweenTickLabelsProperty
            = DependencyProperty.Register("GapBetweenTickLabels", typeof(double), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(0.0) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        public double ChartBottomMargin
        {
            get { return (double)GetValue(ChartBottomMarginProperty); }
            set { SetValue(ChartBottomMarginProperty, value); }
        }

        public static readonly DependencyProperty ChartBottomMarginProperty
            = DependencyProperty.Register("ChartBottomMargin", typeof(double), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(15.0) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        public double ChartTopMargin
        {
            get { return (double)GetValue(ChartTopMarginProperty); }
            set { SetValue(ChartTopMarginProperty, value); }
        }

        public static readonly DependencyProperty ChartTopMarginProperty
            = DependencyProperty.Register("ChartTopMargin", typeof(double), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(15.0) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        public double PriceTickFontSize
        {
            get { return (double)GetValue(PriceTickFontSizeProperty); }
            set { SetValue(PriceTickFontSizeProperty, value); }
        }

        public static readonly DependencyProperty PriceTickFontSizeProperty
            = Twm.Chart.Classes.Chart.PriceTickFontSizeProperty.AddOwner(typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.Inherits) { AffectsRender = true });

        //---------------------------------------------------------------------------------------------------------------------------------------
        private Pen _axisTickPen;

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            "TextColor", typeof(Brush), typeof(ValueTicksElement),
            new FrameworkPropertyMetadata(default(Brush), new PropertyChangedCallback((TextColorChanged))));


        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TickSizeProperty =
            DependencyProperty.Register(nameof(TickSize), typeof(double),
                typeof(ValueTicksElement), new FrameworkPropertyMetadata(0.0));

        public double TickSize
        {
            get => (double)GetValue(TickSizeProperty);
            set => SetValue(TickSizeProperty, value);
        }


        private static void TextColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueTicksElement thisElement = (ValueTicksElement)d;
            Brush newBrushValue = (Brush)e.NewValue;
            if (newBrushValue == null)
            {
                return;
            }

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement._axisTickPen = p;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement._axisTickPen = p;
            }
        }

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public Brush AxisTickColor
        {
            get { return (Brush)GetValue(AxisTickColorProperty); }
            set { SetValue(AxisTickColorProperty, value); }
        }

        public static readonly DependencyProperty AxisTickColorProperty
            = DependencyProperty.Register("AxisTickColor", typeof(Brush), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(Settings.DefaultAxisTickColor, null, CoerceAxisTickColor)
                { AffectsRender = true });

        private static object CoerceAxisTickColor(DependencyObject objWithOldDP, object newDPValue)
        {
            ValueTicksElement thisElement = (ValueTicksElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement._axisTickPen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement._axisTickPen = p;
                return b;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public double PriceAxisWidth
        {
            get { return (double)GetValue(PricePanelWidthProperty); }
            set { SetValue(PricePanelWidthProperty, value); }
        }

        public static readonly DependencyProperty PricePanelWidthProperty
            = DependencyProperty.Register("PriceAxisWidth", typeof(double), typeof(ValueTicksElement),
                new FrameworkPropertyMetadata(0.0) { AffectsRender = true });
        //---------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            var paneView = Classes.Chart.FindParent<UserControl>(this);
            if (paneView is PaneControl p)
            {
                PaneControl = p;
            }
        }

        readonly DrawingGroup _backingStore = new DrawingGroup();

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Render();
            drawingContext.DrawDrawing(_backingStore);
        }

        // Call render anytime, to update visual
        // without triggering layout or OnRender()
        public void Render()
        {
            DrawingContext drawingContext = null;
            try
            {
                drawingContext = _backingStore.Open();
                Render(drawingContext);
            }
            catch (Exception e)
            {
            }
            finally
            {
                drawingContext?.Close();
            }
        }



        private void Render(DrawingContext drawingContext)
        {
            if (Chart != null && (Chart.NotRender || Chart.IsDestroying))
            {
                return;
            }

            drawingContext.DrawRectangle(Background, null, new Rect(new Point(0, 0), RenderSize));
            var highValue = VisibleValuesExtremums.CandleValueHigh;
            var lowValue = VisibleValuesExtremums.CandleValueLow;
            if (!IsMainPane)
            {
                VisibleValuesExtremums.ValuesHigh.TryGetValue(PaneControl, out highValue);
                VisibleValuesExtremums.ValuesLow.TryGetValue(PaneControl, out lowValue);
            }


            double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Verdana"), PriceTickFontSize, Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
            double halfTextHeight = textHeight / 2.0;
            double candlePanelWidth = ActualWidth - PriceAxisWidth;
            double tick_text_X = candlePanelWidth + TICK_LINE_WIDTH + TICK_LEFT_MARGIN;
            double tick_line_endX = candlePanelWidth + TICK_LINE_WIDTH;

            if (highValue != 0 && lowValue != 0)
            {
                var t = 0;
            }

            double chartHeight = ActualHeight - ChartBottomMargin - ChartTopMargin;
            double stepInRubles = (highValue - lowValue) / chartHeight * (2 * (textHeight + GapBetweenTickLabels));
            double stepInRubles_maxDigit = MyWpfMath.MaxDigit(stepInRubles);

            stepInRubles = Math.Ceiling(stepInRubles / stepInRubles_maxDigit) * stepInRubles_maxDigit;
            var stepK = 1;
            if (IsMainPane)
            {
                stepK = (int)(stepInRubles / TickSize);
                if (stepK == 0)
                    stepK = 1;
                stepInRubles = TickSize;
            }


            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            double chartHeight_candlesLHRange_Ratio = chartHeight / (highValue - lowValue);

            void DrawPriceTick(double price)
            {
                FormattedText priceTickFormattedText = new FormattedText(price.ToString(),
                    CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                    PriceTickFontSize, TextColor, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                double y = ChartTopMargin + (highValue - price) * chartHeight_candlesLHRange_Ratio;
                drawingContext.DrawText(priceTickFormattedText, new Point(tick_text_X, y - halfTextHeight));

                if (IsGridlinesEnabled && GridlinesPen != null)
                    drawingContext.DrawLine(GridlinesPen, new Point(PaneWidth * -1, y), new Point(0, y));
            }

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            double theMostRoundPrice = MyWpfMath.TheMostRoundValueInsideRange(lowValue, highValue);
            DrawPriceTick(theMostRoundPrice);

            double maxPriceThreshold = highValue + (ChartTopMargin - halfTextHeight) / chartHeight_candlesLHRange_Ratio;
            double minPriceThreshold =
                highValue + (ChartTopMargin - ActualHeight + halfTextHeight) / chartHeight_candlesLHRange_Ratio;

            int step_i = stepK;
            double next_tick = theMostRoundPrice + step_i * stepInRubles;
            while (next_tick < maxPriceThreshold)
            {
                DrawPriceTick(next_tick);
                step_i += stepK;
                next_tick = theMostRoundPrice + step_i * stepInRubles;
            }

            step_i = stepK;
            next_tick = theMostRoundPrice - step_i * stepInRubles;
            while (next_tick > minPriceThreshold)
            {
                DrawPriceTick(next_tick);
                step_i += stepK;
                next_tick = theMostRoundPrice - step_i * stepInRubles;
            }

            // Горизонтальные линии на всю ширину разделяющая и окаймляющая панели времени и даты:
            drawingContext.DrawLine(GridlinesPen, new Point(0, -100), new Point(0, RenderSize.Height + 100));
            //drawingContext.DrawLine(pen, new Point(0, halfRenderSizeHeight), new Point(RenderSize.Width, halfRenderSizeHeight));
            //drawingContext.DrawLine(pen, new Point(0, RenderSize.Height), new Point(RenderSize.Width, RenderSize.Height));

            var positionDraw = Chart?.PositionDraw;
            if (IsMainPane && Chart != null && positionDraw != null && Chart.IsChartTraderVisible)
            {

                Brush arrowBrush = Brushes.BurlyWood;
                double y = ChartTopMargin + (highValue - positionDraw.Price) * chartHeight_candlesLHRange_Ratio;
                DrawArrowTicket(drawingContext, arrowBrush, positionDraw.Price, y, halfTextHeight, tick_text_X, Chart.TickSize);
            }



            if (IsMainPane && Chart != null && Chart.OrderDraws.Count > 0 && Chart.IsChartTraderVisible)
            {
                foreach (var orderDraw in Chart.OrderDraws.Values)
                {
                    Brush brush = Brushes.Transparent;
                    if (orderDraw.IsStopOrder)
                    {
                        brush = Brushes.Pink;
                    }
                    else if (orderDraw.IsLimitOrder)
                    {
                        brush = Brushes.Cyan;
                    }

                    double y = ChartTopMargin + (highValue - orderDraw.Price) * chartHeight_candlesLHRange_Ratio;
                    DrawArrowTicket(drawingContext, brush, orderDraw.Price, y, halfTextHeight, tick_text_X, Chart.TickSize);
                }

                if (Chart.MoveOrderDraw != null)
                {
                    var orderDraw = Chart.MoveOrderDraw;
                    Brush brush = Chart.ChartBackground;
                    double y = ChartTopMargin + (highValue - orderDraw.Price) * chartHeight_candlesLHRange_Ratio;
                    DrawArrowTicket(drawingContext, brush, orderDraw.Price, y, halfTextHeight, tick_text_X, Chart.TickSize);
                }
            }


            if (IsMainPane && Chart != null && Chart.LastPrice != 0)
            {
                Brush brush = Brushes.LightGray;

                double y = ChartTopMargin + (highValue - Chart.LastPrice) * chartHeight_candlesLHRange_Ratio;
                DrawArrowTicket(drawingContext, brush, Chart.LastPrice, y, halfTextHeight, tick_text_X, Chart.TickSize);


            }
        }

        

        private void DrawArrowTicket(DrawingContext drawingContext, Brush brush, double price, double y, double halfTextHeight, double tick_text_X, double tickSize = 0.01)
        {
            var corY = 3;
            //Arrow ticket
            drawingContext.DrawRectangle(brush, new Pen(brush, 1),
                new Rect(new Point(0, y + halfTextHeight + corY),
                    new Point(RenderSize.Width - 5, y - halfTextHeight - corY)));
            drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(0, y + halfTextHeight + corY), new Point(RenderSize.Width - 5, y + halfTextHeight + corY));
            drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(RenderSize.Width - 5, y + halfTextHeight + corY), new Point(RenderSize.Width - 5, y - halfTextHeight - corY));
            drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(0, y - halfTextHeight - corY), new Point(RenderSize.Width - 5, y - halfTextHeight - corY));

            var segments = new[]
            {
                new LineSegment(new Point(0, y + halfTextHeight + corY), true),
                new LineSegment(new Point(0, y - halfTextHeight - corY), true)
            };

            var figure = new PathFigure(new Point(-5, y), segments, true);
            var geo = new PathGeometry(new[] { figure });


            drawingContext.DrawGeometry(brush, null, geo);
            drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(-5, y), new Point(0, y + halfTextHeight + corY));
            drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(-5, y), new Point(0, y - halfTextHeight - corY));

            FormattedText priceTickFormattedText = new FormattedText(price.ToString("0." + "".PadRight(tickSize.GetDecimalCount(), '0')),
                CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                PriceTickFontSize, TextColor, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            drawingContext.DrawText(priceTickFormattedText, new Point(tick_text_X, y - halfTextHeight));
        }




        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
    }
}