using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;


namespace FancyCandles
{
    class ChartPaneElement : FrameworkElement
    {
        public ChartPaneElement()
        {
            ToolTip tt = new ToolTip() { FontSize = CandleChart.ToolTipFontSize, BorderBrush = Brushes.Beige };
            tt.Content = "";
            ToolTip = tt;

            // Зададим время задержки появления подсказок здесь, а расположение подсказок (если его нужно поменять) зададим в XAML:
            ToolTipService.SetShowDuration(this, int.MaxValue);
            ToolTipService.SetInitialShowDelay(this, 0);

            if (bullishBarPen == null)
            {
                bullishBarPen = new Pen(CandleChart.DefaultBullishVolumeBarFill, 1);
                if (!bullishBarPen.IsFrozen)
                    bullishBarPen.Freeze();
            }

            if (bearishBarPen == null)
            {
                bearishBarPen = new Pen(CandleChart.DefaultBearishVolumeBarFill, 1);
                if (!bearishBarPen.IsFrozen)
                    bearishBarPen.Freeze();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty SeriesSourceProperty
             = DependencyProperty.Register("SeriesSource", typeof(ObservableCollection<ISeriesValue>), typeof(ChartPaneElement), new FrameworkPropertyMetadata(null));
        public ObservableCollection<ISeriesValue> SeriesSource
        {
            get { return (ObservableCollection<ISeriesValue>)GetValue(SeriesSourceProperty); }
            set { SetValue(SeriesSourceProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        private Pen bullishBarPen;

        public static readonly DependencyProperty BullishBarFillProperty
            = DependencyProperty.Register("BullishBarFill", typeof(Brush), typeof(ChartPaneElement), 
                new FrameworkPropertyMetadata(CandleChart.DefaultBullishVolumeBarFill, null, CoerceBullishFill) { AffectsRender = true });
        public Brush BullishBarFill
        {
            get { return (Brush)GetValue(BullishBarFillProperty); }
            set { SetValue(BullishBarFillProperty, value); }
        }

        private static object CoerceBullishFill(DependencyObject objWithOldDP, object newDPValue)
        {
            ChartPaneElement thisElement = (ChartPaneElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement.bullishBarPen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement.bullishBarPen = p;
                return b;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        private Pen bearishBarPen;

        /*
        public static readonly DependencyProperty BearishBarFillProperty
            = DependencyProperty.Register("BearishBarFill", typeof(Brush), typeof(VolumeChartElement), 
                new FrameworkPropertyMetadata(CandleChart.DefaultBearishVolumeBarFill, null, CoerceBearishCandleFill) { AffectsRender = true });
        public Brush BearishBarFill
        {
            get { return (Brush)GetValue(BearishBarFillProperty); }
            set { SetValue(BearishBarFillProperty, value); }
        }

        private static object CoerceBearishCandleFill(DependencyObject objWithOldDP, object newDPValue)
        {
            VolumeChartElement thisElement = (VolumeChartElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement.bearishBarPen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement.bearishBarPen = p;
                return b;
            }
        }*/
        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VisibleCandlesExtremumsProperty
            = DependencyProperty.Register("VisibleCandlesExtremums", typeof(CandleExtremums), typeof(ChartPaneElement), new FrameworkPropertyMetadata(new CandleExtremums(0.0, 0.0, 0L, 0L)) { AffectsRender = true });
        public CandleExtremums VisibleCandlesExtremums
        {
            get { return (CandleExtremums)GetValue(VisibleCandlesExtremumsProperty); }
            set { SetValue(VisibleCandlesExtremumsProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VisibleSeriesRangeProperty
             = DependencyProperty.Register("VisibleSeriesRange", typeof(IntRange), typeof(ChartPaneElement), new FrameworkPropertyMetadata(IntRange.Undefined));
        public IntRange VisibleSeriesRange
        {
            get { return (IntRange)GetValue(VisibleSeriesRangeProperty); }
            set { SetValue(VisibleSeriesRangeProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CandleWidthAndGapProperty
             = DependencyProperty.Register("CandleWidthAndGap", typeof(CandleDrawingParameters), typeof(ChartPaneElement), new FrameworkPropertyMetadata(new CandleDrawingParameters()));
        public CandleDrawingParameters CandleWidthAndGap
        {
            get { return (CandleDrawingParameters)GetValue(CandleWidthAndGapProperty); }
            set { SetValue(CandleWidthAndGapProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        protected override void OnRender(DrawingContext drawingContext)
        {
            //drawingContext.DrawRectangle(Brushes.Aquamarine, transparentPen, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

            for (int i = 0; i < VisibleSeriesRange.Count; i++)
            {
                var value = SeriesSource[VisibleSeriesRange.Start_i + i];
                Brush brush =  BullishBarFill ;
                Pen pen = bullishBarPen;

                double pointY = (1.0 - value.V / (double)VisibleCandlesExtremums.VolumeHigh) * RenderSize.Height;

                double pointLeftX = i * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);

                drawingContext.DrawEllipse(brush, pen, new Point(pointLeftX, pointY), 10, 10 );

                
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            //Vector uv = new Vector(mousePos.X/ RenderSize.Width, mousePos.Y / RenderSize.Height);
            int cndl_i = VisibleSeriesRange.Start_i + (int)(mousePos.X / (CandleWidthAndGap.Width + CandleWidthAndGap.Gap));
            var value = SeriesSource[cndl_i];
            string tooltipText = $"{value.t.ToString("d.MM.yyyy H:mm")}\nV={value.V}";
            ((ToolTip)ToolTip).Content = tooltipText;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
    }
}
