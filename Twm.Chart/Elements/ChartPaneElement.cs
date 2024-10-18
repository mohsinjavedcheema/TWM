using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.DrawObjects;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace Twm.Chart.Elements
{
    class ChartPaneElement : FrameworkElement
    {
        public Classes.Chart Chart { get; set; }

        //private double _lastXPosition;
        //private bool _startedHere;
        private List<Plot> _histoPlots;

        public static double TICK_LINE_WIDTH = 3.0;
        public static double TICK_LEFT_MARGIN = 2.0;
        public static double TICK_RIGHT_MARGIN = 1.0;

        //TODO: possibly need to be as a dependency property
        //  private Pen _candleStrokePen = new Pen(Brushes.Black, 1);

        private Pen _tradeProfitStrokePen = new Pen(Brushes.Green, 2) { DashStyle = DashStyles.Dot };
        private Pen _tradeLossStrokePen = new Pen(Brushes.Red, 2) { DashStyle = DashStyles.Dot };


        private const int _arrowSize = 4;

        private Dictionary<PlotChartType, Action<DrawingContext>> renderers;

        private readonly Dictionary<string, PathGeometry> _orderCloseBoxes;

        private readonly Dictionary<string, PathGeometry> _orderQntBoxes;


        private readonly Dictionary<string, PathGeometry> _orderMoveGeometries;


        private readonly Dictionary<string, PathGeometry> _drawMoveGeometries;

        //---------------------------------------------------------------------------------------------------------------------------------------
        public ChartPaneElement()
        {
            Plots = new ObservableCollection<Plot>();

            DataContextChanged += ChartPaneElement_DataContextChanged;
            Loaded += ChartPaneElement_Loaded;

            if (bullishCandleStrokePen == null)
            {
                bullishCandleStrokePen = new Pen(Settings.DefaultBullishCandleFill, 1);
                if (!bullishCandleStrokePen.IsFrozen)
                    bullishCandleStrokePen.Freeze();
            }

            if (bearishCandleStrokePen == null)
            {
                bearishCandleStrokePen = new Pen(Settings.DefaultBearishCandleFill, 1);
                if (!bearishCandleStrokePen.IsFrozen)
                    bearishCandleStrokePen.Freeze();
            }

            _orderCloseBoxes = new Dictionary<string, PathGeometry>();
            _orderQntBoxes = new Dictionary<string, PathGeometry>();
            _orderMoveGeometries = new Dictionary<string, PathGeometry>();
            _drawMoveGeometries = new Dictionary<string, PathGeometry>();

        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            var paneView = Classes.Chart.FindParent<UserControl>(this);
            if (paneView is PaneControl p)
            {
                PaneControl = p;
                RefreshPlots();
            }
        }

        private void ChartPaneElement_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlots();
        }





        public void RefreshPlots()
        {
            if (PaneControl == null || Chart == null)
                return;

            if (Plots == null)
                Plots = new ObservableCollection<Plot>();


            if (Chart.PanePlots.TryGetValue(PaneControl, out var selfPlots))
            {
                Plots = selfPlots;
                OnSymbolChanged(this, new DependencyPropertyChangedEventArgs());
            }
        }


        private void ChartPaneElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is Classes.Chart chart)
            {
                Chart = chart;
                RefreshPlots();
            }
        }

        public static readonly DependencyProperty PlotNameProperty = DependencyProperty.Register(
            "PlotName", typeof(string), typeof(ChartPaneElement), new PropertyMetadata(default(string)));

        public string PlotName
        {
            get { return (string)GetValue(PlotNameProperty); }
            set { SetValue(PlotNameProperty, value); }
        }

        public static readonly DependencyProperty IsMainPaneProperty
            = DependencyProperty.Register("IsMainPane", typeof(bool), typeof(ChartPaneElement),
                new UIPropertyMetadata(null));

        public bool IsMainPane
        {
            get { return (bool)GetValue(IsMainPaneProperty); }
            set { SetValue(IsMainPaneProperty, value); }
        }

        public static readonly DependencyProperty PaneControlProperty
            = DependencyProperty.Register("PaneControl", typeof(PaneControl), typeof(ChartPaneElement),
                new UIPropertyMetadata(null));

        public PaneControl PaneControl
        {
            get { return (PaneControl)GetValue(PaneControlProperty); }
            set { SetValue(PaneControlProperty, value); }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CandlesSourceProperty
            = DependencyProperty.Register("CandlesSource", typeof(ObservableCollection<ICandle>),
                typeof(ChartPaneElement), new UIPropertyMetadata(null));

        public ObservableCollection<ICandle> CandlesSource
        {
            get { return (ObservableCollection<ICandle>)GetValue(CandlesSourceProperty); }
            set { SetValue(CandlesSourceProperty, value); }
        }


        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(nameof(Symbol), typeof(String),
                typeof(ChartPaneElement), new UIPropertyMetadata(null, OnSymbolChanged));


        public String Symbol
        {
            get => (String)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        public ObservableCollection<Plot> Plots
        {
            get => _plots;
            set
            {
                _plots = value;
                SplitToBars();
            }
        }

        public static readonly DependencyProperty SeriesSourceProperty
            = DependencyProperty.Register("SeriesSource", typeof(ObservableCollection<ISeries<double>>),
                typeof(ChartPaneElement), new UIPropertyMetadata(null, OnSeriesSourceChanged));

        public ObservableCollection<ISeries<double>> SeriesSource
        {
            get { return (ObservableCollection<ISeries<double>>)GetValue(SeriesSourceProperty); }
            set { SetValue(SeriesSourceProperty, value); }
        }

        private static void OnSeriesSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ChartPaneElement chartPane = obj as ChartPaneElement;
            if (chartPane == null) return;

            if (e.OldValue != null)
            {
                ObservableCollection<ISeries<double>> old_obsCollection =
                    (ObservableCollection<ISeries<double>>)e.OldValue;
                old_obsCollection.CollectionChanged -= chartPane.OnseriesSourceCollectionChanged;
            }

            if (e.NewValue != null)
            {
                ObservableCollection<ISeries<double>> new_obsCollection =
                    (ObservableCollection<ISeries<double>>)e.NewValue;
                new_obsCollection.CollectionChanged += chartPane.OnseriesSourceCollectionChanged;
            }

            if (chartPane.IsLoaded)
            {
                chartPane.RefreshPlots();
            }
        }


        // Произошли изменения содержимого коллекции SeriesSource:
        private void OnseriesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //different kind of changes that may have occurred in collection
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                /*if (e.NewStartingIndex == (CandlesSource.Count - 1))
                {
                    //if (CandlesSource.Count > 1)
                    //    ReCalc_FinishedCandlesExtremums_AfterNewFinishedCandleAdded(CandlesSource[CandlesSource.Count - 2]);

                    if ((VisibleCandlesRange.Start_i + VisibleCandlesRange.Count) == e.NewStartingIndex)
                        VisibleCandlesRange = new IntRange(VisibleCandlesRange.Start_i + 1, VisibleCandlesRange.Count);
                }*/
            }

            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                /*int vc_i = e.NewStartingIndex - VisibleCandlesRange.Start_i; // VisibleCandles index.
                if (vc_i >= 0 && vc_i < VisibleCandlesRange.Count)
                    ReCalc_VisibleCandlesExtremums_AfterOneCandleChanged(e.NewStartingIndex);*/
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

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TradeBuyArrowFillProperty
            = DependencyProperty.Register("TradeBuyArrowFill", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultTradeBuyArrowFill, null, CoerceTradeBuyArrowFill)
                { AffectsRender = true });

        public Brush TradeBuyArrowFill
        {
            get { return (Brush)GetValue(TradeBuyArrowFillProperty); }
            set { SetValue(TradeBuyArrowFillProperty, value); }
        }

        private static object CoerceTradeBuyArrowFill(DependencyObject objWithOldDP, object newDPValue)
        {
            Brush newBrushValue = (Brush)newDPValue;
            return newBrushValue.IsFrozen ? newDPValue : newBrushValue.GetCurrentValueAsFrozen();
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty TradeSellArrowFillProperty
            = DependencyProperty.Register("TradeSellArrowFill", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultTradeSellArrowFill, null, CoerceTradeSellArrowFill)
                { AffectsRender = true });

        public Brush TradeSellArrowFill
        {
            get { return (Brush)GetValue(TradeSellArrowFillProperty); }
            set { SetValue(TradeSellArrowFillProperty, value); }
        }

        private static object CoerceTradeSellArrowFill(DependencyObject objWithOldDP, object newDPValue)
        {
            Brush newBrushValue = (Brush)newDPValue;
            return newBrushValue.IsFrozen ? newDPValue : newBrushValue.GetCurrentValueAsFrozen();
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty MarketTextColorProperty = DependencyProperty.Register(
            "MarketTextColor", typeof(Brush), typeof(ChartPaneElement),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush MarketTextColor
        {
            get { return (Brush)GetValue(MarketTextColorProperty); }
            set { SetValue(MarketTextColorProperty, value); }
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty BullishCandleFillProperty
            = DependencyProperty.Register("BullishCandleFill", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultBullishCandleFill, null, CoerceBullishCandleFill)
                { AffectsRender = true });

        public Brush BullishCandleFill
        {
            get { return (Brush)GetValue(BullishCandleFillProperty); }
            set { SetValue(BullishCandleFillProperty, value); }
        }

        private static object CoerceBullishCandleFill(DependencyObject objWithOldDP, object newDPValue)
        {
            Brush newBrushValue = (Brush)newDPValue;
            return newBrushValue.IsFrozen ? newDPValue : newBrushValue.GetCurrentValueAsFrozen();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty BullishCandleStrokeProperty
            = DependencyProperty.Register("BullishCandleStroke", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultBullishCandleFill, null, CoerceBullishCandleStroke)
                { AffectsRender = true });

        public Brush BullishCandleStroke
        {
            get { return (Brush)GetValue(BullishCandleStrokeProperty); }
            set { SetValue(BullishCandleStrokeProperty, value); }
        }

        private Pen bullishCandleStrokePen;

        private static object CoerceBullishCandleStroke(DependencyObject objWithOldDP, object newDPValue)
        {
            ChartPaneElement thisElement = (ChartPaneElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement.bullishCandleStrokePen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement.bullishCandleStrokePen = p;
                return b;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty WickCandleStrokeProperty
            = DependencyProperty.Register("WickCandleStroke", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultWickCandleFill, null, CoerceWickCandleStroke)
                { AffectsRender = true });

        public Brush WickCandleStroke
        {
            get { return (Brush)GetValue(WickCandleStrokeProperty); }
            set { SetValue(WickCandleStrokeProperty, value); }
        }

        private Pen wickCandleStrokePen;

        private static object CoerceWickCandleStroke(DependencyObject objWithOldDP, object newDPValue)
        {
            ChartPaneElement thisElement = (ChartPaneElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement.wickCandleStrokePen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement.wickCandleStrokePen = p;
                return b;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty BearishCandleFillProperty
            = DependencyProperty.Register("BearishCandleFill", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultBearishCandleFill, null, CoerceBearishCandleFill)
                { AffectsRender = true });

        public Brush BearishCandleFill
        {
            get { return (Brush)GetValue(BearishCandleFillProperty); }
            set { SetValue(BearishCandleFillProperty, value); }
        }

        private static object CoerceBearishCandleFill(DependencyObject objWithOldDP, object newDPValue)
        {
            Brush newBrushValue = (Brush)newDPValue;
            return newBrushValue.IsFrozen ? newDPValue : newBrushValue.GetCurrentValueAsFrozen();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty BearishCandleStrokeProperty
            = DependencyProperty.Register("BearishCandleStroke", typeof(Brush), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(Settings.DefaultBearishCandleFill, null, CoerceBearishCandleStroke)
                { AffectsRender = true });

        public Brush BearishCandleStroke
        {
            get { return (Brush)GetValue(BearishCandleStrokeProperty); }
            set { SetValue(BearishCandleStrokeProperty, value); }
        }

        private Pen bearishCandleStrokePen;
        private ObservableCollection<Plot> _plots;

        private static object CoerceBearishCandleStroke(DependencyObject objWithOldDP, object newDPValue)
        {
            ChartPaneElement thisElement = (ChartPaneElement)objWithOldDP;
            Brush newBrushValue = (Brush)newDPValue;

            if (newBrushValue.IsFrozen)
            {
                Pen p = new Pen(newBrushValue, 1.0);
                p.Freeze();
                thisElement.bearishCandleStrokePen = p;
                return newDPValue;
            }
            else
            {
                Brush b = (Brush)newBrushValue.GetCurrentValueAsFrozen();
                Pen p = new Pen(b, 1.0);
                p.Freeze();
                thisElement.bearishCandleStrokePen = p;
                return b;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ChartPaneElement chartPane))
            {
                return;
            }

            chartPane.PlotName = String.Join(", ", chartPane.Plots.Where(plot =>
            {
                if (plot.Owner == null)
                {
                    return false;
                }

                dynamic indicator = plot.Owner;

                if (indicator.Options == null)
                {
                    return true;
                }

                return indicator.Options.ShowPlots;
            }).Select(plot => plot.Owner).Distinct().Select(c =>
            {
                dynamic indicator = c;
                return indicator.Name;
            }));
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VisibleValuesExtremumsProperty
            = DependencyProperty.Register("VisibleValuesExtremums", typeof(ValuesExtremums), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(new ValuesExtremums(0.0, 0.0)) { AffectsRender = true });

        public ValuesExtremums VisibleValuesExtremums
        {
            get { return (ValuesExtremums)GetValue(VisibleValuesExtremumsProperty); }
            set { SetValue(VisibleValuesExtremumsProperty, value); }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty VisibleCandlesRangeProperty
            = DependencyProperty.Register("VisibleCandlesRange", typeof(IntRange), typeof(ChartPaneElement),
                new FrameworkPropertyMetadata(IntRange.Undefined));

        public IntRange VisibleCandlesRange
        {
            get { return (IntRange)GetValue(VisibleCandlesRangeProperty); }
            set { SetValue(VisibleCandlesRangeProperty, value); }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CandleWidthAndGapProperty
            = DependencyProperty.Register("CandleWidthAndGap", typeof(CandleDrawingParameters),
                typeof(ChartPaneElement), new FrameworkPropertyMetadata(new CandleDrawingParameters()));

        public CandleDrawingParameters CandleWidthAndGap
        {
            get { return (CandleDrawingParameters)GetValue(CandleWidthAndGapProperty); }
            set { SetValue(CandleWidthAndGapProperty, value); }
        }

        private void SplitToBars()
        {
            if (Plots == null)
            {
                _histoPlots = new List<Plot>();
            }

            _histoPlots = Plots?.Where(plot => plot.ChartType == PlotChartType.Bars).ToList();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
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

        double _maxH = double.MinValue;
        double _minL = double.MaxValue;

        private void Render(DrawingContext drawingContext)
        {
            if (Chart != null && (Chart.NotRender || Chart.IsDestroying))
                return;

            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), RenderSize));

            double valueRange = VisibleValuesExtremums.CandleValueHigh - VisibleValuesExtremums.CandleValueLow;


            //_histoPlots = Plots.Where(plot => plot.ChartType == PlotChartType.Bars).ToList();

            var dicr = new Dictionary<Plot, Point>();
            foreach (var plot in Plots)
            {
                dicr.Add(plot, new Point(-1, -1));
            }

            _maxH = double.MinValue;
            _minL = double.MaxValue;
            RenderCandles(drawingContext, valueRange, CandleWidthAndGap.Width - 1.0);

            for (int i = 0; i < VisibleCandlesRange.Count; i++)
            {
                RenderHistogramPlots(drawingContext, i, valueRange);
                RenderCommonPlots(drawingContext, i, valueRange, dicr);
            }


            //Draw horizontal lines
            if (Chart != null && Chart.PaneHLineDraws != null && Chart.PaneHLineDraws.TryGetValue(PaneControl.Id, out var lineDraws))
            {
                var lowValue = VisibleValuesExtremums.CandleValueLow;
                var range = valueRange;
                if (!IsMainPane)
                {
                    lowValue = VisibleValuesExtremums.GetValuesLow(PaneControl);
                    range = VisibleValuesExtremums.GetValuesHigh(PaneControl) -
                            VisibleValuesExtremums.GetValuesLow(PaneControl);
                }

                foreach (var lineDraw in lineDraws)
                {
                    var line = lineDraw.Value;
                    var pen = new Pen(line.Brush, line.Width) { DashStyle = line.DashStyle };

                    double rightX = Chart.MaxVisibleRangeCandlesCount * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double y = (1.0 - (line.StartY - lowValue) / range) *
                               RenderSize.Height;
                    drawingContext.DrawLine(pen, new Point(0, y), new Point(rightX, y));
                    var p = new Point(rightX, y);
                    CreateLineGeometric(drawingContext, line, new Point(0, y), p, p);

                }
            }

            RenderPosition(drawingContext, valueRange);
            RenderOrders(drawingContext, valueRange);
            RenderCurrentDraw(drawingContext, valueRange);
            //RenderTexts(drawingContext, valueRange);

            if (IsMainPane)
                if (VisibleValuesExtremums.CandleValueLow != _minL || VisibleValuesExtremums.CandleValueHigh != _maxH)
                    Chart.ReCalc_VisibleCandlesExtremums(_minL, _maxH);
        }

        private void CreateRectGeometric(DrawingContext drawingContext, RectDraw rectDraw, Rect rect)
        {
            if (rectDraw.IsDrawingObject)
            {
                LineSegment[] moveLineSegments = new[]
                    {
                            new LineSegment(new Point(rect.Left, rect.Top), true),
                            new LineSegment(new Point(rect.Right, rect.Top), true),
                            new LineSegment(new Point(rect.Right, rect.Bottom), true),
                            new LineSegment(new Point(rect.Left, rect.Bottom), true)
                    };

                var moveLineFigure = new PathFigure(new Point(rect.Left, rect.Top), moveLineSegments, true);
                var moveLineGeo = new PathGeometry(new[] { moveLineFigure });

                if (_drawMoveGeometries.ContainsKey(rectDraw.Tag))
                {
                    _drawMoveGeometries[rectDraw.Tag] = moveLineGeo;
                }
                else
                {
                    _drawMoveGeometries.Add(rectDraw.Tag, moveLineGeo);
                }

                if (rectDraw.IsSelected)
                {
                    var pen = new Pen(Brushes.Black, 2);


                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(rect.Left, rect.Top), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(rect.Right, rect.Top), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(rect.Right, rect.Bottom), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(rect.Left, rect.Bottom), 2, 2);

                }
            }
        }

        private void CreateLineGeometric(DrawingContext drawingContext, LineDraw line, Point pointStart, Point pointEnd, Point realPointEnd)
        {
            if (line.IsDrawingObject)
            {
                LineSegment[] moveLineSegments;

                if (line.IsVertical)
                {   //Vertical
                    moveLineSegments = new[]
                    {
                            new LineSegment(new Point(pointStart.X+2, pointStart.Y), true),
                            new LineSegment(new Point(pointEnd.X+2, pointEnd.Y), true),
                            new LineSegment(new Point(pointEnd.X-2, pointEnd.Y), true),
                            new LineSegment(new Point(pointStart.X-2, pointStart.Y), true),
                   };

                }
                else if (line.IsHorizontal)
                {   //Horizontal
                    moveLineSegments = new[]
                    {
                            new LineSegment(new Point(pointStart.X, pointStart.Y+2), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y+2), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y-2), true),
                            new LineSegment(new Point(pointStart.X, pointStart.Y-2), true),
                   };
                }
                else
                {
                    moveLineSegments = new[]
                   {
                            new LineSegment(new Point(pointStart.X, pointStart.Y-3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y-3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y+3), true),
                            new LineSegment(new Point(pointStart.X, pointStart.Y+3), true),
                   };
                }
                var moveLineFigure = new PathFigure(new Point(pointStart.X, pointStart.Y + 2), moveLineSegments, true);
                var moveLineGeo = new PathGeometry(new[] { moveLineFigure });

                if (_drawMoveGeometries.ContainsKey(line.Tag))
                {
                    _drawMoveGeometries[line.Tag] = moveLineGeo;
                }
                else
                {
                    _drawMoveGeometries.Add(line.Tag, moveLineGeo);
                }

                if (line.IsSelected)
                {
                    var pen = new Pen(Brushes.Black, 2);

                    if (line is RayDraw)
                    {
                        pointEnd = realPointEnd;
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + (pointEnd.X - pointStart.X) / 2, pointStart.Y + (pointEnd.Y - pointStart.Y) / 2), 2, 2);
                    }
                    else if (line.IsVertical)
                    {   //Vertical
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X, pointStart.Y - 2), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X, pointEnd.Y + 2), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X, pointStart.Y / 2), 2, 2);
                    }
                    else if (line.IsHorizontal)
                    {
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point((pointEnd.X - pointStart.X) / 2, pointStart.Y), 2, 2);
                    }
                    else
                    {
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + (pointEnd.X - pointStart.X) / 2, pointStart.Y + (pointEnd.Y - pointStart.Y) / 2), 2, 2);
                    }
                }
            }
        }


        private void CreateRulerGeometric(DrawingContext drawingContext, RulerDraw ruler, Point pointStart, Point pointEnd, Point pointInfo)
        {
            if (ruler.IsDrawingObject)
            {
                LineSegment[] moveLineSegments = new[]
                   {
                            new LineSegment(new Point(pointStart.X, pointStart.Y-3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y-3), true),
                            new LineSegment(new Point(pointInfo.X, pointInfo.Y-3), true),
                            new LineSegment(new Point(pointInfo.X, pointInfo.Y+3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y+3), true),
                            new LineSegment(new Point(pointStart.X, pointStart.Y+3), true),
                   };

                var moveLineFigure = new PathFigure(new Point(pointStart.X, pointStart.Y + 2), moveLineSegments, true);
                var moveLineGeo = new PathGeometry(new[] { moveLineFigure });

                if (_drawMoveGeometries.ContainsKey(ruler.Tag))
                {
                    _drawMoveGeometries[ruler.Tag] = moveLineGeo;
                }
                else
                {
                    _drawMoveGeometries.Add(ruler.Tag, moveLineGeo);
                }

                if (ruler.IsSelected)
                {
                    var pen = new Pen(Brushes.Black, 2);

                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + (pointEnd.X - pointStart.X) / 2, pointStart.Y + (pointEnd.Y - pointStart.Y) / 2), 2, 2);

                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointInfo.X - 2, pointInfo.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X + (pointInfo.X - pointEnd.X) / 2, pointEnd.Y + (pointInfo.Y - pointEnd.Y) / 2), 2, 2);
                }
            }
        }


        private void CreateRiskGeometric(DrawingContext drawingContext, RiskDraw risk, Point pointStart, Point pointEnd, Point pointReward)
        {
            if (risk.IsDrawingObject)
            {
                LineSegment[] moveLineSegments = new[]
                   {

                            new LineSegment(new Point(pointReward.X, pointReward.Y-3), true),
                            new LineSegment(new Point(pointStart.X, pointStart.Y-3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y-3), true),
                            new LineSegment(new Point(pointEnd.X, pointEnd.Y+3), true),
                            new LineSegment(new Point(pointStart.X, pointStart.Y+3), true),
                            new LineSegment(new Point(pointReward.X, pointReward.Y+3), true),
                   };

                var moveLineFigure = new PathFigure(new Point(pointReward.X, pointReward.Y - 3), moveLineSegments, true);
                var moveLineGeo = new PathGeometry(new[] { moveLineFigure });

                if (_drawMoveGeometries.ContainsKey(risk.Tag))
                {
                    _drawMoveGeometries[risk.Tag] = moveLineGeo;
                }
                else
                {
                    _drawMoveGeometries.Add(risk.Tag, moveLineGeo);
                }

                if (risk.IsSelected)
                {
                    var pen = new Pen(Brushes.Black, 2);

                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointReward.X - 2, pointReward.Y), 2, 2);
                }
            }
        }


        private void RenderCurrentDraw(DrawingContext drawingContext, double valueRange)
        {
            if (Chart.CurrentDraw != null)
            {
                if (Chart.CurrentDraw is RayDraw rayDraw)
                {
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double startX = (rayDraw.BarIndexStart - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double startY = (1.0 - (rayDraw.StartY - lowValue) / valueRange) * RenderSize.Height;


                    double endY = (1.0 - (rayDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                    double endX = (rayDraw.BarIndexEnd - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    var pointEnd = new Point(endX, endY);

                    var dx = endX - startX;
                    var dy = endY - startY;

                    var step = Math.Abs(dx) > Math.Abs(dy) ? 2000 / Math.Abs(dx) : 2000 / Math.Abs(dy);

                    endX += dx * step;
                    endY += dy * step;

                    var pointStart = new Point(startX, startY);

                    drawingContext.DrawLine(new Pen(rayDraw.Brush, Chart.DrawingWidth), pointStart, new Point(endX, endY));

                    var pen = new Pen(Brushes.Black, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + (pointEnd.X - pointStart.X) / 2, pointStart.Y + (pointEnd.Y - pointStart.Y) / 2), 2, 2);

                }
                else if (Chart.CurrentDraw is LineDraw lineDraw)
                {
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double startY = (1.0 - (lineDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                    double endY = (1.0 - (lineDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                    double startX = (lineDraw.BarIndexStart - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double endX = (lineDraw.BarIndexEnd - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);

                    var pointStart = new Point(startX, startY);
                    var pointEnd = new Point(endX, endY);

                    drawingContext.DrawLine(new Pen(lineDraw.Brush, Chart.DrawingWidth), pointStart, pointEnd);

                    var pen = new Pen(Brushes.Black, 2);

                    if (lineDraw.IsVertical)
                    {   //Vertical
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X, pointStart.Y - 2), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X, pointEnd.Y + 2), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X, pointStart.Y / 2), 2, 2);
                    }
                    else if (lineDraw.IsHorizontal)
                    {
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point((pointEnd.X - pointStart.X) / 2, pointStart.Y), 2, 2);
                    }
                    else
                    {
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + 2, pointStart.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointEnd.X - 2, pointEnd.Y), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(pointStart.X + (pointEnd.X - pointStart.X) / 2, pointStart.Y + (pointEnd.Y - pointStart.Y) / 2), 2, 2);
                    }


                }
                else if (Chart.CurrentDraw is RectDraw rectDraw)
                {
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double startY = (1.0 - (rectDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                    double endY = (1.0 - (rectDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                    double startX = (rectDraw.BarIndexStart - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double endX = (rectDraw.BarIndexEnd - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);

                    drawingContext.DrawRectangle(Brushes.Transparent, new Pen(rectDraw.OutlineBrush, Chart.DrawingWidth), new Rect(new Point(startX, startY),
                        new Point(endX, endY)));
                }


                else if (Chart.CurrentDraw is RulerDraw rulerDraw)
                {
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double startY = (1.0 - (rulerDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                    double endY = (1.0 - (rulerDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                    double startX = (rulerDraw.BarIndexStart - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double endX = (rulerDraw.BarIndexEnd - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);

                    drawingContext.DrawLine(new Pen(rulerDraw.Brush, Chart.DrawingWidth), new Point(startX, startY),
                        new Point(endX, endY));

                    if (rulerDraw.IsRulerFixed)
                    {
                        double infoY = (1.0 - (rulerDraw.InfoY - lowValue) / valueRange) * RenderSize.Height;
                        double infoX = (rulerDraw.InfoBarIndex - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);

                        drawingContext.DrawLine(new Pen(rulerDraw.Brush, Chart.DrawingWidth),
                        new Point(endX, endY), new Point(infoX, infoY));


                        var text = Chart.Symbol + "\r\n" + "# bars: " + (rulerDraw.BarIndexEnd - rulerDraw.BarIndexStart) + "\r\n" + "Y value: " +
                            Math.Round(rulerDraw.EndY - rulerDraw.StartY, 2) + "\r\n";

                        FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                       new Typeface("Arial"), 12, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                        { TextAlignment = TextAlignment.Left };

                        var textPoint = new Point(infoX, infoY);

                        var textRect = new Rect(new Point(textPoint.X - 2, textPoint.Y - 2),
                            new Size(ft.Width + 2, ft.Height + 2));


                        var areaBrush = Chart.ChartBackground.CloneCurrentValue();
                        areaBrush.Opacity = 0.5;
                        areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();

                        drawingContext.DrawRectangle(areaBrush, new Pen(Brushes.Black, 1), textRect);
                        drawingContext.DrawText(ft, textPoint);



                    }
                }
                else if (Chart.CurrentDraw is RiskDraw riskDraw)
                {
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double startY = (1.0 - (riskDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                    double endY = (1.0 - (riskDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                    //double rewardY = (1.0 - (riskDraw.RewardY - lowValue) / valueRange) * RenderSize.Height;
                    double startX = (riskDraw.BarIndexStart - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double endX = (riskDraw.BarIndexEnd - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    //double rewardX = (riskDraw.RewardBarIndex - VisibleCandlesRange.Start_i) * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    drawingContext.DrawLine(new Pen(riskDraw.Brush, Chart.DrawingWidth), new Point(startX, startY), new Point(endX, endY));
                    //drawingContext.DrawLine(new Pen(riskDraw.Brush, Chart.DrawingWidth), new Point(rewardX, rewardY), new Point(startX, startY));

                    //var leftX = Math.Min(rewardX, Math.Max(startX, endX));
                    //var rightX = Math.Max(rewardX, Math.Max(startX, endX));
                    drawingContext.DrawLine(new Pen(Brushes.Orange, Chart.DrawingWidth), new Point(startX, startY), new Point(endX, startY));
                    drawingContext.DrawLine(new Pen(Brushes.Red, Chart.DrawingWidth), new Point(startX, endY), new Point(endX, endY));




                }

            }

        }

        private void RenderText(DrawingContext drawingContext, double valueRange)
        {
            if (IsMainPane && Chart != null)
            {
                /* Chart.PaneTextDraws

                 foreach (var textDraw in Chart.TextDraws)
                 {

                 }*/
            }
        }

        private void RenderOrders(DrawingContext drawingContext, double valueRange)
        {
            if (IsMainPane && Chart != null && Chart.OrderDraws.Count > 0 && Chart.IsChartTraderVisible)
            {
                foreach (var orderDraw in Chart.OrderDraws.Values)
                {


                    var priceTickFontSize = 10.0;
                    var highValue = VisibleValuesExtremums.CandleValueHigh;
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                    double halfTextHeight = textHeight / 2.0;

                    var corY = 3;
                    Brush brush = Brushes.Transparent;
                    if (orderDraw.IsStopOrder)
                    {
                        brush = Brushes.Pink;
                    }
                    else if (orderDraw.IsLimitOrder)
                    {
                        brush = Brushes.Cyan;
                    }

                    double y = (1.0 - (orderDraw.Price - lowValue) / valueRange) *
                               RenderSize.Height;

                    var panelWidth = RenderSize.Width;
                    var orderBoxRightMargin = 250;
                    var orderBoxWidth = 130;
                    var orderBoxLeft = orderBoxRightMargin + orderBoxWidth;

                    //Line
                    drawingContext.DrawLine(new Pen(brush, 2), new Point(panelWidth, y),
                        new Point(panelWidth - orderBoxRightMargin, y));

                    //Close box
                    var clRightMargin = 10;
                    var clBoxSize = 16;
                    var qntBoxSize = 55;

                    var closeBoxSegments = new[]
                    {
                        new LineSegment(new Point(panelWidth - (clRightMargin), y + clBoxSize /2), true),
                        new LineSegment(new Point(panelWidth - (clBoxSize + clRightMargin), y + clBoxSize /2), true),
                        new LineSegment(new Point(panelWidth - (clBoxSize + clRightMargin), y - clBoxSize/2), true),
                        new LineSegment(new Point(panelWidth - (clRightMargin), y - clBoxSize /2), true),

                    };

                    var figure = new PathFigure(new Point(panelWidth - clRightMargin, y + clBoxSize / 2), closeBoxSegments, true);
                    var geo = new PathGeometry(new[] { figure });
                    drawingContext.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 1), geo);
                    if (_orderCloseBoxes.ContainsKey(orderDraw.Guid))
                    {
                        _orderCloseBoxes[orderDraw.Guid] = geo;
                    }
                    else
                    {
                        _orderCloseBoxes.Add(orderDraw.Guid, geo);
                    }

                    var crossMargin = 3;
                    drawingContext.DrawLine(new Pen(Brushes.Black, 3), new Point(panelWidth - (clRightMargin + crossMargin), y + (clBoxSize / 2 - crossMargin)),
                        new Point(panelWidth - (clRightMargin + clBoxSize - crossMargin), y - (clBoxSize / 2 - crossMargin)));

                    drawingContext.DrawLine(new Pen(Brushes.Black, 3), new Point(panelWidth - (clRightMargin + crossMargin), y - (clBoxSize / 2 - crossMargin)),
                        new Point(panelWidth - (clRightMargin + clBoxSize - crossMargin), y + (clBoxSize / 2 - crossMargin)));

                    //Order box
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 1),
                        new Rect(new Point(panelWidth - orderBoxLeft, y + halfTextHeight + corY),
                            new Point(panelWidth - orderBoxRightMargin, y - halfTextHeight - corY)));

                    //Qnt box
                    var qntBoxSegments = new[]
                    {
                        new LineSegment(new Point(panelWidth - orderBoxLeft, y - halfTextHeight - corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxLeft + qntBoxSize, y - halfTextHeight - corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxLeft + qntBoxSize, y + halfTextHeight + corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxLeft, y + halfTextHeight + corY), true),
                    };
                    var qntBoxFigure = new PathFigure(new Point(panelWidth - orderBoxLeft, y - halfTextHeight - corY), qntBoxSegments, true);
                    var qntBoxGeo = new PathGeometry(new[] { qntBoxFigure });
                    if (_orderQntBoxes.ContainsKey(orderDraw.Guid))
                    {
                        _orderQntBoxes[orderDraw.Guid] = qntBoxGeo;
                    }
                    else
                    {
                        _orderQntBoxes.Add(orderDraw.Guid, qntBoxGeo);
                    }

                    //Qnt value
                    FormattedText qntFormattedText = new FormattedText(orderDraw.Qnt.ToString(),
                        CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                        priceTickFontSize, MarketTextColor, VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    drawingContext.DrawText(qntFormattedText,
                        new Point(panelWidth - orderBoxLeft + 5, y - halfTextHeight));

                    //Order name
                    FormattedText orderNameFormattedText = new FormattedText(orderDraw.Name.ToString(),
                        CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                        priceTickFontSize, MarketTextColor, VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    drawingContext.DrawText(orderNameFormattedText,
                        new Point(panelWidth - orderBoxLeft + 55, y - halfTextHeight));



                    var moveOrderSegments = new[]
                    {
                        new LineSegment(new Point(panelWidth - orderBoxRightMargin, y+2), true),
                        new LineSegment(new Point(panelWidth, y+2), true),
                        new LineSegment(new Point(panelWidth, y-2), true),
                        new LineSegment(new Point(panelWidth - orderBoxRightMargin, y-2), true),

                        new LineSegment(new Point(panelWidth - orderBoxRightMargin, y - halfTextHeight - corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxLeft + qntBoxSize, y - halfTextHeight - corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxLeft + qntBoxSize, y + halfTextHeight + corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxRightMargin, y + halfTextHeight + corY), true),
                        new LineSegment(new Point(panelWidth - orderBoxRightMargin, y+2), true),
                    };
                    var moveOrderFigure = new PathFigure(new Point(panelWidth - orderBoxRightMargin, y + 2), moveOrderSegments, true);
                    var moveOrderGeo = new PathGeometry(new[] { moveOrderFigure });


                    if (_orderMoveGeometries.ContainsKey(orderDraw.Guid))
                    {
                        _orderMoveGeometries[orderDraw.Guid] = moveOrderGeo;
                    }
                    else
                    {
                        _orderMoveGeometries.Add(orderDraw.Guid, moveOrderGeo);
                    }

                    if (!Chart.FixedScale)
                    {
                        double chartHeight = this.RenderSize.Height;
                        var tickSizeHeight = chartHeight / ((Chart.VisibleValuesExtremums.CandleValueHigh - Chart.VisibleValuesExtremums.CandleValueLow) / Chart.TickSize);
                        var tickSizeCount = (int)((textHeight) / tickSizeHeight) + 2;

                        if (orderDraw.Price <= _minL)
                            _minL = orderDraw.Price - Chart.TickSize * tickSizeCount;


                        if (orderDraw.Price >= _maxH)
                            _maxH = orderDraw.Price + Chart.TickSize * tickSizeCount;
                    }

                }


                if (Chart.MoveOrderDraw != null)
                {
                    var orderDraw = Chart.MoveOrderDraw;
                    var priceTickFontSize = 10.0;
                    var highValue = VisibleValuesExtremums.CandleValueHigh;
                    var lowValue = VisibleValuesExtremums.CandleValueLow;
                    double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                    double halfTextHeight = textHeight / 2.0;

                    var corY = 3;
                    Brush brush = Brushes.Black;

                    double y = (1.0 - (orderDraw.Price - lowValue) / valueRange) * RenderSize.Height;

                    var panelWidth = RenderSize.Width;
                    var orderBoxRightMargin = 250;
                    var orderBoxWidth = 130;
                    var orderBoxLeft = orderBoxRightMargin + orderBoxWidth;
                    var clRightMargin = 10;
                    var clBoxSize = 16;

                    //Line
                    drawingContext.DrawLine(new Pen(brush, 1), new Point(panelWidth, y),
                        new Point(panelWidth - (clRightMargin), y));

                    drawingContext.DrawLine(new Pen(brush, 1), new Point(panelWidth - (clRightMargin + clBoxSize), y),
                        new Point(panelWidth - orderBoxRightMargin, y));


                    //Close box
                    var closeBoxSegments = new[]
                    {
                        new LineSegment(new Point(panelWidth - (clRightMargin), y + clBoxSize /2), true),
                        new LineSegment(new Point(panelWidth - (clBoxSize + clRightMargin), y + clBoxSize /2), true),
                        new LineSegment(new Point(panelWidth - (clBoxSize + clRightMargin), y - clBoxSize/2), true),
                        new LineSegment(new Point(panelWidth - (clRightMargin), y - clBoxSize /2), true),

                    };

                    var figure = new PathFigure(new Point(panelWidth - clRightMargin, y + clBoxSize / 2), closeBoxSegments, true);
                    var geo = new PathGeometry(new[] { figure });
                    drawingContext.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Black, 1), geo);

                    //Order box
                    drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 1),
                        new Rect(new Point(panelWidth - orderBoxLeft, y + halfTextHeight + corY),
                            new Point(panelWidth - orderBoxRightMargin, y - halfTextHeight - corY)));

                }



            }
        }

        private void RenderPosition(DrawingContext drawingContext, double valueRange)
        {
            var positionDraw = Chart?.PositionDraw;

            if (IsMainPane && Chart != null && positionDraw != null && Chart.IsChartTraderVisible)
            {
                var priceTickFontSize = 10.0;
                var highValue = VisibleValuesExtremums.CandleValueHigh;
                var lowValue = VisibleValuesExtremums.CandleValueLow;
                double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                double halfTextHeight = textHeight / 2.0;

                var corY = 3;
                Brush arrowBrush = Brushes.BurlyWood;

                double y = (1.0 - (positionDraw.Price - lowValue) / valueRange) *
                           RenderSize.Height;

                var pandelwidth = RenderSize.Width;

                //Line
                drawingContext.DrawLine(new Pen(arrowBrush, 2), new Point(pandelwidth, y),
                    new Point(pandelwidth - 150, y));
                //Pnl box
                drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1),
                    new Rect(new Point(pandelwidth - 250, y + halfTextHeight + corY),
                        new Point(pandelwidth - 150, y - halfTextHeight - corY)));
                //Qnt box
                drawingContext.DrawRectangle(Chart.BullishCandleFill, new Pen(Brushes.Black, 1),
                    new Rect(new Point(pandelwidth - 150, y + halfTextHeight + corY),
                        new Point(pandelwidth - 120, y - halfTextHeight - corY)));

                //Pnl value
                var pnlTextBrush = positionDraw.Pnl > 0 ? Chart.BullishCandleFill : Brushes.Red;
                FormattedText pnlFormattedText = new FormattedText((positionDraw.Pnl ?? 0).ToString("0.00 $"),
                    CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                    priceTickFontSize, pnlTextBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                var pnlTextHalfWidth = pnlFormattedText.Width / 2.0;
                drawingContext.DrawText(pnlFormattedText,
                    new Point(pandelwidth - 200 - pnlTextHalfWidth, y - halfTextHeight));

                //Qnt value
                FormattedText qntFormattedText = new FormattedText(positionDraw.Qnt.ToString(),
                    CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                    priceTickFontSize, MarketTextColor, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                var qntTextHalfWidth = qntFormattedText.Width / 2.0;
                drawingContext.DrawText(qntFormattedText,
                    new Point(pandelwidth - 135 - qntTextHalfWidth, y - halfTextHeight));


                if (!Chart.FixedScale)
                {
                    double chartHeight = this.RenderSize.Height;
                    var tickSizeHeight = chartHeight / ((Chart.VisibleValuesExtremums.CandleValueHigh - Chart.VisibleValuesExtremums.CandleValueLow) / Chart.TickSize);
                    var tickSizeCount = (int)((textHeight) / tickSizeHeight) + 2;

                    if (positionDraw.Price <= _minL)
                        _minL = positionDraw.Price - Chart.TickSize * tickSizeCount;


                    if (positionDraw.Price >= _maxH)
                        _maxH = positionDraw.Price + Chart.TickSize * tickSizeCount;
                }

            }
        }

        private void RenderCandles(DrawingContext drawingContext, double valueRange, double correctedCndlWidth)
        {
            if (IsMainPane)
            {
                var tradeTexts = new Dictionary<Point, TextDraw>(new PointEqualityComparer());

                var customTexts = new List<KeyValuePair<Point, TextDraw>>();

                // Chart.ReCalc_VisibleCandlesRange

                for (int i = 0; i < Chart.MaxVisibleRangeCandlesCount; i++)
                {
                    double cndlLeftX = i * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                    double cndlCenterX = cndlLeftX + 0.5 * CandleWidthAndGap.Width;
                    if (i < VisibleCandlesRange.Count)
                    {
                        ICandle cndl = CandlesSource[VisibleCandlesRange.Start_i + i];
                        Brush cndlBrush = (cndl.C > cndl.O) ? BullishCandleFill : BearishCandleFill;

                        var defaultCandleBrush = cndlBrush;

                        Chart.BarColors.TryGetValue(VisibleCandlesRange.Start_i + i, out var candleBrush);
                        cndlBrush = candleBrush ?? defaultCandleBrush;

                        Chart.BarBackgroundsColors.TryGetValue(VisibleCandlesRange.Start_i + i, out var barBackgroundBrush);

                        //Pen cndlPen = (cndl.C > cndl.O) ? bullishCandleStrokePen : bearishCandleStrokePen;
                        double wnd_L = (1.0 - (cndl.L - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                       RenderSize.Height;
                        double wnd_H = (1.0 - (cndl.H - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                       RenderSize.Height;
                        double wnd_O = (1.0 - (cndl.O - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                       RenderSize.Height;
                        double wnd_C = (1.0 - (cndl.C - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                       RenderSize.Height;
                        drawingContext.DrawLine(wickCandleStrokePen, new Point(cndlCenterX, wnd_L),
                        new Point(cndlCenterX, wnd_H));

                        double cndlBodyH = Math.Abs(wnd_O - wnd_C);

                        if (barBackgroundBrush != null)
                        {
                            drawingContext.DrawRectangle(barBackgroundBrush, null,
                                new Rect(new Point(cndlLeftX - CandleWidthAndGap.Gap, 0),
                                    new Size(CandleWidthAndGap.Width + (2 * CandleWidthAndGap.Gap), ActualHeight)));
                        }
                        Pen cndlStrokePen = (cndl.C > cndl.O) ? bullishCandleStrokePen : bearishCandleStrokePen;
                        if (cndlBodyH > 1.0)
                        {
                            drawingContext.DrawRectangle(cndlBrush, cndlStrokePen,
                                new Rect(cndlLeftX + 0.5, Math.Min(wnd_O, wnd_C) + 0.5, correctedCndlWidth,
                                    cndlBodyH - 1.0));
                        }
                        else
                            drawingContext.DrawLine(cndlStrokePen, new Point(cndlLeftX, wnd_O),
                                new Point(cndlLeftX + CandleWidthAndGap.Width, wnd_O));
                        DrawCandleArrows(drawingContext, i, cndlCenterX, valueRange, wnd_H, wnd_L, tradeTexts, customTexts);
                        DrawLines(drawingContext, i, cndlCenterX, valueRange, wnd_H, wnd_L);

                        if (cndl.H > _maxH) _maxH = cndl.H;
                        if (cndl.L < _minL) _minL = cndl.L;
                    }
                    else
                    {
                        if (Chart.PaneTextDraws.TryGetValue(PaneControl.Id, out var textDraws))
                        {
                            if (textDraws.TryGetValue(VisibleCandlesRange.Start_i + i, out var drawTextIds))
                            {
                                foreach (var drawTextId in drawTextIds)
                                {
                                    var drawText = Chart.TextDraws[drawTextId];

                                    var textY =
                                        (1.0 - (drawText.Y - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                        RenderSize.Height;

                                    var point = new Point(cndlCenterX, textY);
                                    customTexts.Add(new KeyValuePair<Point, TextDraw>(point, drawText));
                                }
                            }
                        }
                        DrawLines(drawingContext, i, cndlCenterX, valueRange, 0, 0);
                    }

                }



                foreach (var text in tradeTexts)
                {
                    var drawText = text.Value;
                    var point = text.Key;
                    DrawText(drawingContext, drawText.Text, point, drawText.TextBrush, drawText.FontName,
                        drawText.TextSize, drawText.OutlineBrush, drawText.AreaBrush, drawText.AreaOpacity,
                        drawText.YTop, drawText);
                }

                foreach (var text in customTexts)
                {
                    var drawText = text.Value;
                    var point = text.Key;
                    DrawText(drawingContext, drawText.Text, point, drawText.TextBrush, drawText.FontName,
                        drawText.TextSize, drawText.OutlineBrush, drawText.AreaBrush, drawText.AreaOpacity,
                        drawText.YTop, drawText);
                }
            }
        }


        private void DrawCandleArrows(DrawingContext drawingContext, int candleIndex, double cndlCenterX,
            double valueRange, double cndlTop, double cndlBottom, Dictionary<Point, TextDraw> texts,
            List<KeyValuePair<Point, TextDraw>> customTexts)
        {
            ICandle cndl = CandlesSource[VisibleCandlesRange.Start_i + candleIndex];

            //Trade Arrows
            if (Chart.PlotExecutions > 0 &&
                Chart.BarTrades.TryGetValue(VisibleCandlesRange.Start_i + candleIndex, out var barTradeList))
            {
                //TODO: Marker brush color
                var textBrush = MarketTextColor;
                var textSize = _arrowSize * 2.5;
                var outlineBrush = Brushes.Transparent;
                var areaBrush = Brushes.Transparent;
                var opacity = 1;
                var fontName = "Veranda";

                double chartHeight = this.RenderSize.Height;
                var tickSizeHeight = chartHeight / ((Chart.VisibleValuesExtremums.CandleValueHigh - Chart.VisibleValuesExtremums.CandleValueLow) / Chart.TickSize);

                foreach (var tradeId in barTradeList)
                {
                    var tradeInfo = Chart.TradeDraws[tradeId];

                    var text = tradeInfo.EntryOrderName + "\r\n" + tradeInfo.EntryQuantity + " @ " +
                               tradeInfo.EntryPrice;

                    //Entry arrow >
                    if (tradeInfo.EntryBar - 1 == VisibleCandlesRange.Start_i + candleIndex)
                    {
                        Brush arrowBrush;
                        if (!tradeInfo.IsLong)
                        {
                            //Enter Short
                            arrowBrush = TradeSellArrowFill;
                            DrawArrow(drawingContext, ArrowDirection.Down, arrowBrush,
                                new Point(cndlCenterX, cndlTop - 2), true);

                            var tickSizeCount = (int)(_arrowSize / tickSizeHeight) + 1;

                            if (cndl.H + Chart.TickSize * tickSizeCount >= _maxH)
                            {
                                _maxH = cndl.H + Chart.TickSize * tickSizeCount;
                            }


                            if (Chart.PlotExecutions == 1)
                            {
                                double aSize = _arrowSize * 3 + 4;
                                var point = new Point(cndlCenterX, cndlTop - aSize);
                                FormattedText ft;

                                while (texts.ContainsKey(point))
                                {
                                    var td = texts[point];
                                    ft = new FormattedText(td.Text, CultureInfo.InvariantCulture,
                                            FlowDirection.LeftToRight,
                                            new Typeface(fontName), textSize, textBrush,
                                            VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Center };


                                    point = new Point(cndlCenterX, point.Y - 2 - ft.Height);

                                    aSize = aSize + ft.Height + 2;
                                }

                                ft = new FormattedText(text, CultureInfo.InvariantCulture,
                                         FlowDirection.LeftToRight,
                                         new Typeface(fontName), textSize, textBrush,
                                         VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                { TextAlignment = TextAlignment.Center };
                                tickSizeCount = (int)(((aSize + ft.Height)) / tickSizeHeight) + 1;


                                if (cndl.H + Chart.TickSize * tickSizeCount >= _maxH)
                                {

                                    _maxH = cndl.H + Chart.TickSize * tickSizeCount;
                                }


                                texts.Add(point, new TextDraw()
                                {
                                    Text = text,
                                    TextBrush = textBrush,
                                    FontName = fontName,
                                    TextSize = (int)textSize,
                                    OutlineBrush = outlineBrush,
                                    AreaBrush = areaBrush,
                                    AreaOpacity = opacity,
                                    YTop = false
                                });
                            }
                        }
                        else
                        {
                            //Enter Long
                            arrowBrush = TradeBuyArrowFill;
                            DrawArrow(drawingContext, ArrowDirection.Up, arrowBrush,
                                new Point(cndlCenterX, cndlBottom + 2), true);

                            var tickSizeCount = (int)(_arrowSize / tickSizeHeight) + 1;

                            if (cndl.L - Chart.TickSize * tickSizeCount <= _minL)
                            {
                                _minL = cndl.L - Chart.TickSize * tickSizeCount;
                            }

                            if (Chart.PlotExecutions == 1)
                            {
                                double aSize = _arrowSize * 3 + 4;
                                var point = new Point(cndlCenterX, cndlBottom + aSize);
                                FormattedText ft;

                                while (texts.ContainsKey(point))
                                {
                                    var td = texts[point];

                                    ft = new FormattedText(td.Text, CultureInfo.InvariantCulture,
                                            FlowDirection.LeftToRight,
                                            new Typeface(fontName), textSize, textBrush,
                                            VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Center };

                                    point = new Point(cndlCenterX, point.Y + 2 + ft.Height);

                                    aSize = aSize + ft.Height + 2;
                                }

                                ft = new FormattedText(text, CultureInfo.InvariantCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface(fontName), textSize, textBrush,
                                        VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                { TextAlignment = TextAlignment.Center };
                                tickSizeCount = (int)(((aSize + ft.Height)) / tickSizeHeight) + 1;


                                if (cndl.L - Chart.TickSize * tickSizeCount <= _minL)
                                {

                                    _minL = cndl.L - Chart.TickSize * tickSizeCount;
                                }

                                texts.Add(point, new TextDraw()
                                {
                                    Text = text,
                                    TextBrush = textBrush,
                                    FontName = fontName,
                                    TextSize = (int)textSize,
                                    OutlineBrush = outlineBrush,
                                    AreaBrush = areaBrush,
                                    AreaOpacity = opacity,
                                    YTop = true
                                });
                            }
                        }

                        double triangleY =
                            (1.0 - (tradeInfo.EntryPrice - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;
                        DrawArrow(drawingContext, ArrowDirection.Left, arrowBrush, new Point(cndlCenterX, triangleY));


                        if (tradeInfo.ExitBar != null && VisibleCandlesRange.Count - candleIndex <=
                            (tradeInfo.ExitBar - tradeInfo.EntryBar))
                        {
                            //Draw last trade line
                            var pen = tradeInfo.IsProfit ? _tradeProfitStrokePen : _tradeLossStrokePen;

                            double endPointX =
                                (candleIndex + ((tradeInfo.ExitBar ?? 0) - tradeInfo.EntryBar)) *
                                (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                            double endPointY =
                                (1.0 - ((tradeInfo.ExitPrice ?? 0) - VisibleValuesExtremums.CandleValueLow) /
                                 valueRange) * RenderSize.Height;
                            var endPoint = new Point(endPointX, endPointY);
                            drawingContext.DrawLine(pen, new Point(cndlCenterX, triangleY), endPoint);
                        }
                    }

                    text = tradeInfo.ExitOrderName + "\r\n" + tradeInfo.ExitQuantity + " @ " + tradeInfo.ExitPrice;

                    //Exit arrow <
                    if (((tradeInfo.ExitBar ?? 0) - 1) == VisibleCandlesRange.Start_i + candleIndex)
                    {
                        Brush arrowBrush;
                        if (!tradeInfo.IsLong)
                        {
                            //Exit Short
                            arrowBrush = TradeBuyArrowFill;
                            DrawArrow(drawingContext, ArrowDirection.Up, arrowBrush,
                                new Point(cndlCenterX, cndlBottom + 2), true);

                            var tickSizeCount = (int)(_arrowSize / tickSizeHeight) + 1;

                            if (cndl.L - Chart.TickSize * tickSizeCount <= _minL)
                            {
                                _minL = cndl.L - Chart.TickSize * tickSizeCount;
                            }

                            if (Chart.PlotExecutions == 1)
                            {
                                double aSize = _arrowSize * 3 + 4;
                                var point = new Point(cndlCenterX, cndlBottom + aSize);
                                FormattedText ft;

                                while (texts.ContainsKey(point))
                                {
                                    var td = texts[point];

                                    ft = new FormattedText(td.Text, CultureInfo.InvariantCulture,
                                            FlowDirection.LeftToRight,
                                            new Typeface(fontName), textSize, textBrush,
                                            VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Center };


                                    point = new Point(cndlCenterX, point.Y + 2 + ft.Height);

                                    aSize = aSize + ft.Height + 2;
                                }


                                ft = new FormattedText(text, CultureInfo.InvariantCulture,
                                       FlowDirection.LeftToRight,
                                       new Typeface(fontName), textSize, textBrush,
                                       VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                { TextAlignment = TextAlignment.Center };
                                tickSizeCount = (int)(((aSize + ft.Height)) / tickSizeHeight) + 1;


                                if (cndl.L - Chart.TickSize * tickSizeCount <= _minL)
                                {

                                    _minL = cndl.L - Chart.TickSize * tickSizeCount;
                                }

                                texts.Add(
                                    point, new TextDraw()
                                    {
                                        Text = text,
                                        TextBrush = textBrush,
                                        FontName = fontName,
                                        TextSize = (int)textSize,
                                        OutlineBrush = outlineBrush,
                                        AreaBrush = areaBrush,
                                        AreaOpacity = opacity,
                                        YTop = true
                                    });
                            }
                        }
                        else
                        {
                            //Exit Long
                            arrowBrush = TradeSellArrowFill;
                            DrawArrow(drawingContext, ArrowDirection.Down, arrowBrush,
                                new Point(cndlCenterX, cndlTop - 2), true);


                            var tickSizeCount = (int)((_arrowSize) / tickSizeHeight) + 1;

                            if (cndl.H + Chart.TickSize * tickSizeCount >= _maxH)
                            {

                                _maxH = cndl.H + Chart.TickSize * tickSizeCount;
                            }

                            if (Chart.PlotExecutions == 1)
                            {

                                double aSize = _arrowSize * 3 + 4;
                                var point = new Point(cndlCenterX, cndlTop - aSize);
                                FormattedText ft;
                                while (texts.ContainsKey(point))
                                {
                                    var td = texts[point];

                                    ft = new FormattedText(td.Text, CultureInfo.InvariantCulture,
                                           FlowDirection.LeftToRight,
                                           new Typeface(fontName), textSize, textBrush,
                                           VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Center };


                                    point = new Point(cndlCenterX, point.Y - 2 - ft.Height);
                                    aSize = aSize + ft.Height + 2;
                                }

                                ft = new FormattedText(text, CultureInfo.InvariantCulture,
                                           FlowDirection.LeftToRight,
                                           new Typeface(fontName), textSize, textBrush,
                                           VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                { TextAlignment = TextAlignment.Center };
                                tickSizeCount = (int)(((aSize + ft.Height)) / tickSizeHeight) + 1;


                                if (cndl.H + Chart.TickSize * tickSizeCount >= _maxH)
                                {

                                    _maxH = cndl.H + Chart.TickSize * tickSizeCount;
                                }

                                texts.Add(point, new TextDraw()
                                {
                                    Text = text,
                                    TextBrush = textBrush,
                                    FontName = fontName,
                                    TextSize = (int)textSize,
                                    OutlineBrush = outlineBrush,
                                    AreaBrush = areaBrush,
                                    AreaOpacity = opacity,
                                    YTop = false
                                });
                            }
                        }

                        double triangleY =
                            (1.0 - ((tradeInfo.ExitPrice ?? 0) - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;
                        DrawArrow(drawingContext, ArrowDirection.Right, arrowBrush, new Point(cndlCenterX, triangleY));

                        //Draw all trade lines
                        var pen = tradeInfo.IsProfit ? _tradeProfitStrokePen : _tradeLossStrokePen;
                        double startPointX =
                            (candleIndex - ((tradeInfo.ExitBar ?? 0) - tradeInfo.EntryBar)) *
                            (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                        double startPointY =
                            (1.0 - (tradeInfo.EntryPrice - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;
                        var startPoint = new Point(startPointX, startPointY);
                        drawingContext.DrawLine(pen, startPoint, new Point(cndlCenterX, triangleY));
                    }
                }
            }


            //Custom arrows
            if (Chart.PaneArrowDraws.TryGetValue(PaneControl.Id, out var arrowDraws))
            {
                if (arrowDraws.TryGetValue(VisibleCandlesRange.Start_i + candleIndex, out var drawArrowIds))
                {
                    foreach (var drawArrowId in drawArrowIds)
                    {
                        var drawArrow = Chart.ArrowDraws[drawArrowId];

                        var arrowY =
                            (1.0 - (drawArrow.Y - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;

                        var point = new Point(cndlCenterX, arrowY);

                        DrawArrow(drawingContext, drawArrow.Direction, drawArrow.Brush,
                            point, true, drawArrow.Connector == ArrowConnector.End);
                    }
                }
            }

            //Texts
            if (Chart.PaneTextDraws.TryGetValue(PaneControl.Id, out var textDraws))
            {
                if (textDraws.TryGetValue(VisibleCandlesRange.Start_i + candleIndex, out var drawTextIds))
                {
                    foreach (var drawTextId in drawTextIds)
                    {
                        var drawText = Chart.TextDraws[drawTextId];

                        var textY =
                            (1.0 - (drawText.Y - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;

                        var point = new Point(cndlCenterX, textY);
                        customTexts.Add(new KeyValuePair<Point, TextDraw>(point, drawText));
                    }
                }
            }
        }


        private void DrawLines(DrawingContext drawingContext, int candleIndex, double cndlCenterX,
            double valueRange, double cndlTop, double cndlBottom)
        {
            if (Chart.BarLines.TryGetValue(VisibleCandlesRange.Start_i + candleIndex, out var barLineList))
            {
                foreach (var lineTag in barLineList)
                {
                    var line = Chart.LineDraws[lineTag];

                    //Line start
                    if (line.BarIndexStart == VisibleCandlesRange.Start_i + candleIndex &&
                        line.BarIndexStart != line.BarIndexEnd)
                    {
                        if (VisibleCandlesRange.Count - candleIndex <=
                            (line.BarIndexEnd - line.BarIndexStart))
                        {
                            //Draw last line
                            double startPointY =
                                (1.0 - (line.StartY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                RenderSize.Height;
                            double endPointX =
                                (candleIndex + (line.BarIndexEnd - line.BarIndexStart)) *
                                (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                            double endPointY =
                                (1.0 - (line.EndY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                RenderSize.Height;
                            var endPoint = new Point(endPointX, endPointY);

                            if (VisibleCandlesRange.Start_i + Chart.MaxVisibleRangeCandlesCount <= line.BarIndexEnd)
                            {
                                if (line is RayDraw rayDraw)
                                {
                                    var realPoint = new Point(endPoint.X, endPoint.Y);
                                    var dx = endPoint.X - cndlCenterX;
                                    var dy = endPoint.Y - startPointY;
                                    var step = Math.Abs(dx) > Math.Abs(dy) ? 2000 / Math.Abs(dx) : 2000 / Math.Abs(dy);
                                    endPoint.X += dx * step;
                                    endPoint.Y += dy * step;
                                    var pen = new Pen(rayDraw.Brush, rayDraw.Width) { DashStyle = rayDraw.DashStyle };
                                    drawingContext.DrawLine(pen, new Point(cndlCenterX, startPointY), endPoint);
                                    CreateLineGeometric(drawingContext, rayDraw, new Point(cndlCenterX, startPointY), endPoint, realPoint);
                                }
                                else if (line is LineDraw lineDraw)
                                {
                                    var pen = new Pen(lineDraw.Brush, lineDraw.Width) { DashStyle = lineDraw.DashStyle };
                                    drawingContext.DrawLine(pen, new Point(cndlCenterX, startPointY), endPoint);
                                    CreateLineGeometric(drawingContext, lineDraw, new Point(cndlCenterX, startPointY), endPoint, endPoint);
                                }
                                else if (line is RectDraw rectDraw)
                                {
                                    //                               
                                    var areaBrush = rectDraw.AreaBrush.CloneCurrentValue();
                                    areaBrush.Opacity = rectDraw.AreaOpacity;
                                    areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();
                                    var outlineBrush = rectDraw.OutlineBrush.CloneCurrentValue();

                                    var rect = new Rect(new Point(cndlCenterX, startPointY), endPoint);
                                    drawingContext.DrawRectangle(areaBrush, new Pen(outlineBrush, rectDraw.Width) { DashStyle = rectDraw.DashStyle }, rect);
                                    CreateRectGeometric(drawingContext, rectDraw, rect);
                                }
                                else if (line is RulerDraw rulerDraw)
                                {

                                    var pen = new Pen(rulerDraw.Brush, rulerDraw.Width) { DashStyle = rulerDraw.DashStyle };
                                    drawingContext.DrawLine(pen, new Point(cndlCenterX, startPointY), endPoint);

                                    double infoPointX =
                                   (candleIndex + (rulerDraw.InfoBarIndex - rulerDraw.BarIndexEnd)) *
                                   (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                                    double infoPointY =
                                        (1.0 - (rulerDraw.InfoY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                        RenderSize.Height;

                                    var infoPoint = new Point(infoPointX, infoPointY);
                                    drawingContext.DrawLine(new Pen(rulerDraw.Brush, Chart.DrawingWidth), endPoint, infoPoint);


                                    var text = Chart.Symbol + "\r\n" + "# bars: " + (rulerDraw.BarIndexEnd - rulerDraw.BarIndexStart) + "\r\n" + "Y value: " +
                                        Math.Round(rulerDraw.EndY - rulerDraw.StartY, 2) + "\r\n";

                                    FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface("Arial"), 12, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Left };

                                    var textPoint = new Point(infoPointX, infoPointY);

                                    var textRect = new Rect(new Point(textPoint.X - 2, textPoint.Y - 2),
                                        new Size(ft.Width + 2, ft.Height + 2));


                                    var areaBrush = Chart.ChartBackground.CloneCurrentValue();
                                    areaBrush.Opacity = 0.5;
                                    areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();

                                    drawingContext.DrawRectangle(areaBrush, new Pen(Brushes.Black, 1), textRect);
                                    drawingContext.DrawText(ft, textPoint);

                                    CreateRulerGeometric(drawingContext, rulerDraw, new Point(cndlCenterX, startPointY), endPoint, infoPoint);
                                }
                                else if (line is RiskDraw riskDraw)
                                {
                                    var startPoint = new Point(cndlCenterX, startPointY);
                                    var pen = new Pen(riskDraw.Brush, riskDraw.Width) { DashStyle = riskDraw.DashStyle };
                                    drawingContext.DrawLine(pen, startPoint, endPoint);

                                    double rewardPointX =
                                   (candleIndex + (riskDraw.BarIndexStart - riskDraw.RewardBarIndex)) *
                                   (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                                    double rewardPointY =
                                        (1.0 - (riskDraw.RewardY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                        RenderSize.Height;

                                    var rewardPoint = new Point(rewardPointX, rewardPointY);
                                    drawingContext.DrawLine(pen, startPoint, rewardPoint);


                                    var leftX = Math.Min(rewardPointX, Math.Min(startPoint.X, endPoint.X));
                                    var rightX = Math.Max(rewardPointX, Math.Max(startPoint.X, endPoint.X));
                                    drawingContext.DrawLine(new Pen(Brushes.Orange, Chart.DrawingWidth), new Point(leftX, startPoint.Y), new Point(rightX, startPoint.Y));
                                    var textEnter = Math.Round(riskDraw.StartY, 2).ToString();
                                    FormattedText ft = new FormattedText(textEnter, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                    new Typeface("Arial"), 12, Brushes.Orange, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Left };
                                    var textPoint = new Point(leftX, startPoint.Y);
                                    drawingContext.DrawText(ft, textPoint);

                                    drawingContext.DrawLine(new Pen(Brushes.Red, Chart.DrawingWidth), new Point(leftX, endPoint.Y), new Point(rightX, endPoint.Y));
                                    var textRisk = Math.Round(riskDraw.EndY, 2).ToString();
                                    ft = new FormattedText(textRisk, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface("Arial"), 12, Brushes.Red, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Left };
                                    textPoint = new Point(leftX, endPoint.Y);
                                    drawingContext.DrawText(ft, textPoint);

                                    drawingContext.DrawLine(new Pen(Brushes.Green, Chart.DrawingWidth), new Point(leftX, rewardPointY), new Point(rightX, rewardPointY));
                                    var textReward = Math.Round(riskDraw.RewardY, 2).ToString();
                                    ft = new FormattedText(textReward, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface("Arial"), 12, Brushes.Green, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                                    { TextAlignment = TextAlignment.Left };
                                    drawingContext.DrawText(ft, new Point(leftX, rewardPoint.Y));



                                    CreateRiskGeometric(drawingContext, riskDraw, new Point(cndlCenterX, startPointY), endPoint, rewardPoint);
                                }
                            }
                        }
                    }

                    //Line end
                    if (line.BarIndexEnd == VisibleCandlesRange.Start_i + candleIndex)
                    {
                        //Draw all lines
                        Point startPoint;
                        Point endPoint;

                        if (line.BarIndexStart != line.BarIndexEnd)
                        {
                            //Multi bar line
                            double startPointX =
                                (candleIndex - (line.BarIndexEnd - line.BarIndexStart)) *
                                (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                            double startPointY =
                                (1.0 - (line.StartY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                RenderSize.Height;
                            startPoint = new Point(startPointX, startPointY);

                            double endPointY =
                                (1.0 - (line.EndY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                RenderSize.Height;
                            endPoint = new Point(cndlCenterX, endPointY);

                        }
                        else
                        {
                            //One bar line

                            if (line.IsVertical)
                            {
                                //Vertical line
                                double startPointX = candleIndex * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                                double startPointY = (1.0) * RenderSize.Height;
                                double endPointY =
                                    (1.0 - (VisibleValuesExtremums.CandleValueHigh -
                                            VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                    RenderSize.Height;
                                startPoint = new Point(startPointX, startPointY);
                                endPoint = new Point(startPointX, endPointY);
                            }
                            else
                            {

                                double startPointX =
                                    (candleIndex - (line.BarIndexEnd - line.BarIndexStart)) *
                                    (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                                double startPointY =
                                    (1.0 - (line.StartY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                    RenderSize.Height;
                                double endPointY =
                                  (1.0 - (line.EndY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                    RenderSize.Height;
                                startPoint = new Point(startPointX, startPointY);
                                endPoint = new Point(startPointX + CandleWidthAndGap.Width, endPointY);
                            }
                        }

                        if (line is RayDraw rayDraw)
                        {
                            var realEndPoint = new Point(endPoint.X, endPoint.Y);
                            var dx = endPoint.X - startPoint.X;
                            var dy = endPoint.Y - startPoint.Y;
                            var step = Math.Abs(dx) > Math.Abs(dy) ? 2000 / Math.Abs(dx) : 2000 / Math.Abs(dy);
                            endPoint.X += dx * step;
                            endPoint.Y += dy * step;

                            var pen = new Pen(rayDraw.Brush, rayDraw.Width) { DashStyle = rayDraw.DashStyle };
                            drawingContext.DrawLine(pen, startPoint, endPoint);
                            CreateLineGeometric(drawingContext, rayDraw, startPoint, endPoint, realEndPoint);

                        }
                        else if (line is LineDraw lineDraw)
                        {
                            var pen = new Pen(lineDraw.Brush, lineDraw.Width) { DashStyle = lineDraw.DashStyle };
                            drawingContext.DrawLine(pen, startPoint, endPoint);
                            CreateLineGeometric(drawingContext, lineDraw, startPoint, endPoint, endPoint);
                        }
                        else if (line is RectDraw rectDraw)
                        {
                            //
                            var areaBrush = rectDraw.AreaBrush.CloneCurrentValue();
                            areaBrush.Opacity = rectDraw.AreaOpacity;
                            areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();
                            var outlineBrush = rectDraw.OutlineBrush.CloneCurrentValue();
                            var rect = new Rect(startPoint, endPoint);
                            drawingContext.DrawRectangle(areaBrush, new Pen(outlineBrush, rectDraw.Width) { DashStyle = rectDraw.DashStyle }, rect);
                            CreateRectGeometric(drawingContext, rectDraw, rect);
                        }
                        else if (line is RulerDraw rulerDraw)
                        {

                            double infoPointX =
                                cndlCenterX + (rulerDraw.InfoBarIndex - line.BarIndexEnd) *
                                (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                            double infoPointY =
                                (1.0 - (rulerDraw.InfoY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                            RenderSize.Height;
                            var infoPoint = new Point(infoPointX, infoPointY);

                            var pen = new Pen(rulerDraw.Brush, rulerDraw.Width) { DashStyle = rulerDraw.DashStyle };
                            drawingContext.DrawLine(pen, startPoint, endPoint);
                            drawingContext.DrawLine(new Pen(rulerDraw.Brush, Chart.DrawingWidth), endPoint, infoPoint);
                            var text = Chart.Symbol + "\r\n" + "# bars: " + (rulerDraw.BarIndexEnd - rulerDraw.BarIndexStart) + "\r\n" + "Y value: " +
                                Math.Round(rulerDraw.EndY - rulerDraw.StartY, 2) + "\r\n";

                            FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                           new Typeface("Arial"), 12, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                            { TextAlignment = TextAlignment.Left };

                            var textPoint = new Point(infoPointX, infoPointY);

                            var textRect = new Rect(new Point(textPoint.X - 2, textPoint.Y - 2),
                                new Size(ft.Width + 2, ft.Height + 2));


                            var areaBrush = Chart.ChartBackground.CloneCurrentValue();
                            areaBrush.Opacity = 0.5;
                            areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();

                            drawingContext.DrawRectangle(areaBrush, new Pen(Brushes.Black, 1), textRect);
                            drawingContext.DrawText(ft, textPoint);


                            CreateRulerGeometric(drawingContext, rulerDraw, startPoint, endPoint, infoPoint);
                        }
                        else if (line is RiskDraw riskDraw)
                        {

                            var pen = new Pen(riskDraw.Brush, riskDraw.Width) { DashStyle = riskDraw.DashStyle };
                            drawingContext.DrawLine(pen, startPoint, endPoint);

                            double rewardPointX =
                           (candleIndex - (riskDraw.BarIndexEnd - riskDraw.RewardBarIndex)) *
                                (CandleWidthAndGap.Width + CandleWidthAndGap.Gap) + 0.5 * CandleWidthAndGap.Width;
                            double rewardPointY =
                                (1.0 - (riskDraw.RewardY - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                                RenderSize.Height;

                            var rewardPoint = new Point(rewardPointX, rewardPointY);
                            drawingContext.DrawLine(pen, startPoint, rewardPoint);


                            var leftX = Math.Min(rewardPointX, Math.Min(startPoint.X, endPoint.X));
                            var rightX = Math.Max(rewardPointX, Math.Max(startPoint.X, endPoint.X));
                            drawingContext.DrawLine(new Pen(Brushes.Orange, Chart.DrawingWidth), new Point(leftX, startPoint.Y), new Point(rightX, startPoint.Y));
                            var textEnter = Math.Round(riskDraw.StartY, 2).ToString();
                            FormattedText ft = new FormattedText(textEnter, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                            new Typeface("Arial"), 12, Brushes.Orange, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                            { TextAlignment = TextAlignment.Left };
                            var textPoint = new Point(startPoint.X, startPoint.Y);
                            drawingContext.DrawText(ft, textPoint);

                            drawingContext.DrawLine(new Pen(Brushes.Red, Chart.DrawingWidth), new Point(leftX, endPoint.Y), new Point(rightX, endPoint.Y));
                            var textRisk = Math.Round(riskDraw.EndY, 2).ToString();
                            ft = new FormattedText(textRisk, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                           new Typeface("Arial"), 12, Brushes.Red, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                            { TextAlignment = TextAlignment.Left };
                            textPoint = new Point(leftX, endPoint.Y);
                            drawingContext.DrawText(ft, textPoint);

                            drawingContext.DrawLine(new Pen(Brushes.Green, Chart.DrawingWidth), new Point(leftX, rewardPointY), new Point(rightX, rewardPointY));
                            var textReward = Math.Round(riskDraw.RewardY, 2).ToString();
                            ft = new FormattedText(textReward, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                           new Typeface("Arial"), 12, Brushes.Green, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                            { TextAlignment = TextAlignment.Left };
                            drawingContext.DrawText(ft, new Point(leftX, rewardPoint.Y));



                            CreateRiskGeometric(drawingContext, riskDraw, startPoint, endPoint, rewardPoint);
                        }


                        if (line.StartY != -1 && line.EndY != -1)
                        {
                            if (line.StartY <= _minL)
                                _minL = line.StartY - Chart.TickSize;

                            if (line.StartY >= _maxH)
                                _maxH = line.StartY + Chart.TickSize;

                            if (line.EndY <= _minL)
                                _minL = (line.EndY - Chart.TickSize);

                            if (line.EndY >= _maxH)
                                _maxH = line.EndY + Chart.TickSize;
                        }
                    }
                }
            }
        }


        private void DrawText(DrawingContext drawingContext, string text, Point point, Brush textBrush, string fontName,
            double textSize, Brush outlineBrush, Brush areaBrush, double areaOpacity, bool yTop = true, TextDraw drawText = null)
        {
            if (text == null)
                return;
            FormattedText ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(fontName), textSize, textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
            { TextAlignment = TextAlignment.Center };

            var textPoint = new Point(point.X, point.Y);

            if (!yTop)
            {
                textPoint = new Point(textPoint.X, textPoint.Y - ft.Height);
            }

            if (outlineBrush != Brushes.Transparent || areaBrush != Brushes.Transparent)
            {
                areaBrush = areaBrush.CloneCurrentValue();
                areaBrush.Opacity = areaOpacity;
                areaBrush = (Brush)areaBrush.GetCurrentValueAsFrozen();

                if (drawText.IsDrawingObject)
                {
                    ft.TextAlignment = TextAlignment.Left;
                    var textRect = new Rect(new Point(textPoint.X, textPoint.Y),
                    new Size(ft.Width + 2, ft.Height + 2));
                    drawingContext.DrawRectangle(areaBrush, new Pen(outlineBrush, 1), textRect);

                    LineSegment[] moveLineSegments = new[]
                        {
                            new LineSegment(new Point(textRect.Left, textRect.Top), true),
                            new LineSegment(new Point(textRect.Right, textRect.Top), true),
                            new LineSegment(new Point(textRect.Right, textRect.Bottom), true),
                            new LineSegment(new Point(textRect.Left, textRect.Bottom), true)
                    };

                    var moveLineFigure = new PathFigure(new Point(textRect.Left, textRect.Top), moveLineSegments, true);
                    var moveLineGeo = new PathGeometry(new[] { moveLineFigure });

                    if (_drawMoveGeometries.ContainsKey(drawText.Tag))
                    {
                        _drawMoveGeometries[drawText.Tag] = moveLineGeo;
                    }
                    else
                    {
                        _drawMoveGeometries.Add(drawText.Tag, moveLineGeo);
                    }

                    if (drawText.IsSelected)
                    {
                        var pen = new Pen(Brushes.Black, 2);


                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(textRect.Left, textRect.Top), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(textRect.Right, textRect.Top), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(textRect.Right, textRect.Bottom), 2, 2);
                        drawingContext.DrawEllipse(Brushes.Black, pen, new Point(textRect.Left, textRect.Bottom), 2, 2);

                    }
                }
                else
                {
                    var textRect = new Rect(new Point(textPoint.X - 1 - ft.Width / 2, textPoint.Y - 1),
                    new Size(ft.Width + 2, ft.Height + 2));
                    drawingContext.DrawRectangle(areaBrush, new Pen(outlineBrush, 1), textRect);
                }
            }
            drawingContext.DrawText(ft, textPoint);
        }

        private void DrawArrow(DrawingContext drawingContext, ArrowDirection direction, Brush brush, Point endPoint,
            bool drawBody = false, bool isEndPoint = true)
        {
            var arrowLength = _arrowSize * 3;
            var bodyLength = _arrowSize * 2;
            var bodyWidth = (_arrowSize / 2) * 2;
            if (!isEndPoint)
            {
                //Correct Y endPoint to arrow Length
                if (direction == ArrowDirection.Up)
                {
                    endPoint = new Point(endPoint.X, endPoint.Y - arrowLength);
                }

                if (direction == ArrowDirection.Down)
                {
                    endPoint = new Point(endPoint.X, endPoint.Y + arrowLength);
                }
            }


            Point pointLeft = new Point(0, 0),
                pointRight = new Point(0, 0);
            Rect bodyRect = new Rect(0, 0, 0, 0);

            switch (direction)
            {
                case ArrowDirection.Left:
                    pointLeft = new Point(endPoint.X - _arrowSize, endPoint.Y - _arrowSize);
                    pointRight = new Point(endPoint.X - _arrowSize, endPoint.Y + _arrowSize);
                    bodyRect = new Rect(new Point(endPoint.X - arrowLength, endPoint.Y - (int)(bodyWidth / 2)),
                        new Size(bodyLength, bodyWidth));
                    break;
                case ArrowDirection.Right:
                    pointLeft = new Point(endPoint.X + _arrowSize, endPoint.Y + _arrowSize);
                    pointRight = new Point(endPoint.X + _arrowSize, endPoint.Y - _arrowSize);
                    bodyRect = new Rect(new Point(endPoint.X + _arrowSize, endPoint.Y - (int)(bodyWidth / 2)),
                        new Size(bodyLength, bodyWidth));
                    break;
                case ArrowDirection.Up:
                    pointLeft = new Point(endPoint.X - _arrowSize, endPoint.Y + _arrowSize);
                    pointRight = new Point(endPoint.X + _arrowSize, endPoint.Y + _arrowSize);
                    bodyRect = new Rect(new Point(endPoint.X - (int)(bodyWidth / 2), endPoint.Y + _arrowSize),
                        new Size(bodyWidth, bodyLength));
                    break;
                case ArrowDirection.Down:
                    pointLeft = new Point(endPoint.X + _arrowSize, endPoint.Y - _arrowSize);
                    pointRight = new Point(endPoint.X - _arrowSize, endPoint.Y - _arrowSize);
                    bodyRect = new Rect(new Point(endPoint.X - (int)(bodyWidth / 2), endPoint.Y - arrowLength),
                        new Size(bodyWidth, bodyLength));
                    break;
            }

            var segments = new[]
            {
                new LineSegment(pointLeft, true),
                new LineSegment(pointRight, true)
            };

            var figure = new PathFigure(endPoint, segments, true);
            var geo = new PathGeometry(new[] { figure });

            drawingContext.DrawGeometry(brush, null, geo);

            if (drawBody)
            {
                drawingContext.DrawRectangle(brush, null, bodyRect);
            }
        }


        private void RenderHistogramPlots(DrawingContext drawingContext, int candleIndex, double valueRange)
        {
            if (!_histoPlots.Any()) return;
            foreach (var plot in ReorderHistogramPlots(candleIndex))
            {
                if (plot.IsDataSourceEmpty || plot.DataSource.Count < VisibleCandlesRange.Start_i - 1)
                    break;

                var series = plot.DataSource;

                double rangePane = VisibleValuesExtremums.GetValuesHigh(PaneControl) -
                                   VisibleValuesExtremums.GetValuesLow(PaneControl);
                var defaultPenBrush = new SolidColorBrush(plot.Color);
                Pen pen = new Pen(defaultPenBrush, plot.Thickness);
                Brush penBrush = null;
                plot.PlotColors?.TryGetValue(VisibleCandlesRange.Start_i + candleIndex, out penBrush);
                pen.Brush = penBrush ?? defaultPenBrush;

                if (!series.IsValidPoint(VisibleCandlesRange.Start_i + candleIndex))
                    break;

                var value = series.GetValueAt(VisibleCandlesRange.Start_i + candleIndex);


                double pointLeftX = candleIndex * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
                double pointCenterX = pointLeftX + 0.5 * CandleWidthAndGap.Width;

                double pointY;
                var baseValue = RenderSize.Height;
                if (!IsMainPane)
                {
                    pointY = (1.0 - ((double)value - VisibleValuesExtremums.GetValuesLow(PaneControl)) / rangePane) *
                             (RenderSize.Height);

                    if (VisibleValuesExtremums.GetValuesLow(PaneControl) < 0)
                    {
                        baseValue = (1.0 - (0 - VisibleValuesExtremums.GetValuesLow(PaneControl)) / rangePane) *
                                    (RenderSize.Height);
                    }
                }
                else
                {
                    pointY = (1.0 - (value - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                             (RenderSize.Height);
                }

                var currentPoint = new Point(pointCenterX, pointY);
                drawingContext.DrawLine(pen, new Point(currentPoint.X, baseValue), currentPoint);
            }
        }

        private void RenderCommonPlots(DrawingContext drawingContext, int candleIndex, double valueRange,
            Dictionary<Plot, Point> dicr)
        {
            double rangePane = VisibleValuesExtremums.GetValuesHigh(PaneControl) -
                               VisibleValuesExtremums.GetValuesLow(PaneControl);

            double pointLeftX = candleIndex * (CandleWidthAndGap.Width + CandleWidthAndGap.Gap);
            double pointCenterX = pointLeftX + 0.5 * CandleWidthAndGap.Width;

            var index = VisibleCandlesRange.Start_i + candleIndex;

            foreach (var plot in Plots.Except(_histoPlots))
            {
                Point prevPoint = dicr[plot];

                if (plot.IsDataSourceEmpty || plot.DataSource.Count < VisibleCandlesRange.Start_i - 1)
                    continue;

                var series = plot.DataSource;


                if (!series.IsValidPoint(index))
                    break;


                var defaultPenBrush = new SolidColorBrush(plot.Color);
                Pen pen = new Pen(defaultPenBrush, plot.Thickness);
                Brush penBrush = null;
                plot.PlotColors?.TryGetValue(index, out penBrush);
                pen.Brush = penBrush ?? defaultPenBrush;

                if (plot.LineType == PlotLineType.Dashed)
                {
                    pen.DashStyle = DashStyles.Dash;
                }

                /*if (VisibleCandlesRange.Start_i + candleIndex >= series.Count)
                    continue;*/

                var value = series.GetValueAt(index);

                double pointY;

                if (!IsMainPane)
                {
                    pointY = (1.0 - (value - VisibleValuesExtremums.GetValuesLow(PaneControl)) / rangePane) *
                             (RenderSize.Height);
                }
                else
                {
                    pointY = (1.0 - (value - VisibleValuesExtremums.CandleValueLow) / valueRange) *
                             (RenderSize.Height);
                }

                var currentPoint = new Point(pointCenterX, pointY);
                if (prevPoint.X != -1 && prevPoint.Y != -1)
                {
                    drawingContext.DrawLine(pen, prevPoint, currentPoint);
                }

                if (plot.IsAutoscale && value != 0)
                {

                    if (value >= _maxH)
                    {
                        _maxH = value;
                    }

                    if (value <= _minL)
                    {
                        _minL = value;
                    }
                }




                dicr[plot] = currentPoint;
            }

            if (!IsMainPane)
            {
                //Custom arrows
                if (Chart.PaneArrowDraws.TryGetValue(PaneControl.Id, out var arrowDraws))
                {
                    if (arrowDraws.TryGetValue(index, out var drawArrowIds))
                    {
                        foreach (var drawArrowId in drawArrowIds)
                        {
                            var drawArrow = Chart.ArrowDraws[drawArrowId];

                            var arrowY =
                                (1.0 - (drawArrow.Y - VisibleValuesExtremums.GetValuesLow(PaneControl)) / rangePane) *
                                RenderSize.Height;

                            var point = new Point(pointCenterX, arrowY);

                            DrawArrow(drawingContext, drawArrow.Direction, drawArrow.Brush,
                                point, true, drawArrow.Connector == ArrowConnector.End);
                        }
                    }
                }

                //Custom text
                if (Chart.PaneTextDraws.TryGetValue(PaneControl.Id, out var textDraws))
                {
                    if (textDraws.TryGetValue(index, out var drawTextIds))
                    {
                        foreach (var drawTextId in drawTextIds)
                        {
                            var drawText = Chart.TextDraws[drawTextId];

                            var textY =
                                (1.0 - (drawText.Y - VisibleValuesExtremums.GetValuesLow(PaneControl)) / rangePane) *
                                RenderSize.Height;

                            var point = new Point(pointCenterX, textY);

                            DrawText(drawingContext, drawText.Text, point, drawText.TextBrush, drawText.FontName,
                                drawText.TextSize, drawText.OutlineBrush, drawText.AreaBrush, drawText.AreaOpacity,
                                drawText.YTop);
                        }
                    }
                }
            }
        }


        private IEnumerable<Plot> ReorderHistogramPlots(int position)
        {
            return _histoPlots.OrderByDescending(plot =>
            {
                if (plot.IsDataSourceEmpty || plot.DataSource.Count < VisibleCandlesRange.Start_i - 1)
                    return 0;
                var series = plot.DataSource;
                var value = series.GetValueAt(VisibleCandlesRange.Start_i + position);
                return value;
            });
        }


        public void CancelMoveOrderDraw()
        {
            _moveOrderStart = false;
            if (Chart.MoveOrderDraw != null)
            {
                Chart.MoveOrderDraw = null;
                Render();
                Chart.ChartControl.MainPane.valueTicksElement.Render();
            }
        }


        bool _startDrawObject;
        bool _startDrawMove;
        bool _startDrawAdjust;
        Point _startMovePosition;

        public void CancelCurrentDraw()
        {
            _startDrawObject = false;
            if (Chart.CurrentDraw != null)
            {
                Chart.CurrentDraw = null;
                Render();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            CancelMoveOrderDraw();
            CancelCurrentDraw();

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                CancelMoveOrderDraw();
                CancelCurrentDraw();
            }
            else if (e.Key == Key.Delete)
            {
                if (Chart.SelectedDraw != null)
                {
                    Chart.DeleteDrawObject(Chart.SelectedDraw);
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            _startDrawMove = false;
            //_startMovePosition = null;
            if (Chart.AdjustDraw != null)
            {
                if (Chart.AdjustDraw is BaseLineDraw lineDraw)
                {
                    if (lineDraw.IsAdjust)
                    {
                        Chart.AdjustDraw = null;
                        lineDraw.IsAdjust = false;
                        lineDraw.AdjustStart = false;
                        lineDraw.AdjustEnd = false;
                        lineDraw.AdjustPoint = false;
                    }

                }
            }

            if (e.ChangedButton != MouseButton.Left)
            {
                CancelMoveOrderDraw();
                CancelCurrentDraw();
                return;
            }
        }


        private bool CheckStartEndDraw(Point point, Point viewPosition)
        {
            if (Chart.IsLineMode)
            {
                if (_startDrawObject)
                {
                    //Fix line
                    _startDrawObject = false;
                    if (Chart.CurrentDraw is LineDraw line)
                    {
                        Chart.DrawingTools.Line(line.BarIndexStart, line.BarIndexEnd, line.Tag, line.StartY, line.EndY, line.Brush, line.Width, line.DashStyle, line.IsAutoScale, true);
                    }
                    Chart.CurrentDraw = null;
                    Chart.ResetDrawMode();                    
                }
                else
                {
                    //Start draw line
                    _startDrawObject = true;
                    var lineDraw = new LineDraw();
                    var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                    lineDraw.StartY = price ?? 0;
                    lineDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                    lineDraw.EndY = lineDraw.StartY;
                    lineDraw.BarIndexEnd = lineDraw.BarIndexStart;
                    lineDraw.Width = Chart.DrawingWidth;
                    lineDraw.DashStyle = DashStyles.Solid;
                    lineDraw.Brush = new SolidColorBrush(Chart.DrawingColor);
                    lineDraw.Tag = Guid.NewGuid().ToString();
                    Chart.CurrentDraw = lineDraw;                    
                }
            }
            else if (Chart.IsRulerMode)
            {
                if (_startDrawObject)
                {
                    if (Chart.CurrentDraw is RulerDraw rulerDraw)
                    {
                        if (!rulerDraw.IsRulerFixed)
                        {

                            rulerDraw.InfoY = rulerDraw.EndY;
                            rulerDraw.InfoBarIndex = rulerDraw.BarIndexEnd;
                            rulerDraw.IsRulerFixed = true;
                        }
                        else
                        {
                            //Fix  Ruler
                            Chart.DrawingTools.Ruler(rulerDraw.BarIndexStart, rulerDraw.BarIndexEnd, rulerDraw.InfoBarIndex, rulerDraw.Tag, rulerDraw.StartY, rulerDraw.EndY, rulerDraw.InfoY, rulerDraw.Brush, rulerDraw.Width, rulerDraw.DashStyle, rulerDraw.IsAutoScale, true);
                            _startDrawObject = false;
                            Chart.CurrentDraw = null;
                            Chart.ResetDrawMode();
                        }                        
                    }
                }
                else
                {
                    if (Chart.CurrentDraw == null)
                    {
                        //Start Ruler
                        _startDrawObject = true;
                        var rulerDraw = new RulerDraw();
                        var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                        rulerDraw.StartY = price ?? 0;
                        rulerDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                        rulerDraw.EndY = rulerDraw.StartY;
                        rulerDraw.BarIndexEnd = rulerDraw.BarIndexStart;
                        rulerDraw.Width = Chart.DrawingWidth;
                        rulerDraw.DashStyle = DashStyles.Solid;
                        rulerDraw.Brush = new SolidColorBrush(Chart.DrawingColor);
                        rulerDraw.Tag = Guid.NewGuid().ToString();
                        Chart.CurrentDraw = rulerDraw;                      
                    }
                }
            }
            else if (Chart.IsRiskMode)
            {
                if (_startDrawObject)
                {
                    if (Chart.CurrentDraw is RiskDraw riskDraw)
                    {
                        riskDraw.RewardBarIndex = riskDraw.BarIndexStart;
                        riskDraw.ApplyRatio();
                        

                        //Fix Risk
                        Chart.DrawingTools.Risk(riskDraw.BarIndexStart, riskDraw.BarIndexEnd, riskDraw.RewardBarIndex, riskDraw.Tag, riskDraw.StartY, riskDraw.EndY, riskDraw.RewardY, riskDraw.Brush, riskDraw.Width, riskDraw.DashStyle, riskDraw.Ratio, riskDraw.IsAutoScale, true);
                        _startDrawObject = false;
                        Chart.CurrentDraw = null;
                        Chart.ResetDrawMode();                        
                    }
                }
                else
                {
                    if (Chart.CurrentDraw == null)
                    {
                        //Start Ruler
                        _startDrawObject = true;
                        var riskDraw = new RiskDraw();
                        var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                        riskDraw.StartY = price ?? 0;
                        riskDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                        riskDraw.EndY = riskDraw.StartY;
                        riskDraw.BarIndexEnd = riskDraw.BarIndexStart;
                        riskDraw.Width = Chart.DrawingWidth;
                        riskDraw.DashStyle = DashStyles.Solid;
                        riskDraw.Brush = new SolidColorBrush(Chart.DrawingColor);
                        riskDraw.Tag = Guid.NewGuid().ToString();
                        riskDraw.Ratio = 2;
                        Chart.CurrentDraw = riskDraw;
                    }
                }
            }
            else if (Chart.IsRectMode)
            {
                if (_startDrawObject)
                {
                    //Fix rect
                    _startDrawObject = false;
                    if (Chart.CurrentDraw is RectDraw rect)
                    {
                        Chart.DrawingTools.Rect(rect.BarIndexStart, rect.BarIndexEnd, rect.Tag, rect.StartY, rect.EndY, rect.Width, rect.DashStyle, rect.OutlineBrush, rect.AreaBrush, rect.AreaOpacity, rect.IsAutoScale, true);
                    }
                    Chart.CurrentDraw = null;
                    Chart.ResetDrawMode();
                    Render();
                }
                else
                {
                    //Start draw rect
                    _startDrawObject = true;
                    var rectDraw = new RectDraw();
                    var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                    rectDraw.StartY = price ?? 0;
                    rectDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                    rectDraw.EndY = rectDraw.StartY;
                    rectDraw.BarIndexEnd = rectDraw.BarIndexStart;
                    rectDraw.Width = Chart.DrawingWidth;
                    rectDraw.OutlineBrush = new SolidColorBrush(Chart.DrawingColor);
                    rectDraw.DashStyle = DashStyles.Solid;
                    rectDraw.AreaBrush = Brushes.Transparent;
                    rectDraw.Tag = Guid.NewGuid().ToString();
                    Chart.CurrentDraw = rectDraw;                    
                }
            }
            else if (Chart.IsRayMode)
            {
                if (_startDrawObject)
                {
                    //Fix ray
                    _startDrawObject = false;
                    if (Chart.CurrentDraw is RayDraw ray)
                    {
                        Chart.DrawingTools.Ray(ray.BarIndexStart, ray.BarIndexEnd, ray.Tag, ray.StartY, ray.EndY, ray.Brush, ray.Width, ray.DashStyle, ray.IsAutoScale, true);
                    }
                    Chart.CurrentDraw = null;
                    Chart.ResetDrawMode();                    
                }
                else
                {
                    //Start draw ray
                    _startDrawObject = true;
                    var rayDraw = new RayDraw();
                    var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                    rayDraw.StartY = price ?? 0;
                    rayDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                    rayDraw.EndY = rayDraw.StartY;
                    rayDraw.BarIndexEnd = rayDraw.BarIndexStart;
                    rayDraw.Width = Chart.DrawingWidth;
                    rayDraw.Brush = new SolidColorBrush(Chart.DrawingColor);
                    rayDraw.Tag = Guid.NewGuid().ToString();
                    Chart.CurrentDraw = rayDraw;                    
                }

            }
            else if (Chart.IsHLineMode)
            {
                //Fix h line
                var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                Chart.DrawingTools.LineHorizontal(Guid.NewGuid().ToString(), price ?? 0, new SolidColorBrush(Chart.DrawingColor), Chart.DrawingWidth, DashStyles.Solid, null, false, true);
                Chart.ResetDrawMode();                
            }
            else if (Chart.IsVLineMode)
            {
                //Fix v line
                var barIndexStart = Chart.GetBarIndexFromPoint(point);
                Chart.DrawingTools.LineVertical(Guid.NewGuid().ToString(), barIndexStart, new SolidColorBrush(Chart.DrawingColor), Chart.DrawingWidth, DashStyles.Solid, true);
                Chart.ResetDrawMode();                
            }
            else if (Chart.IsTextMode)
            {
                //Fix text
                var viewPos = viewPosition;
                var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);
                var barIndexStart = Chart.GetBarIndexFromPoint(point);
                var textTag = Guid.NewGuid().ToString();
                Chart.DrawingTools.Text(barIndexStart, textTag, Chart.DrawingText, 0, price ?? 0, Brushes.Black, "", Chart.DrawingSize, Brushes.Black, Brushes.White, 1, true, null, true);
                var textDraw = Chart.SelectedDraw as TextDraw;

                double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                          FlowDirection.LeftToRight, new Typeface("Verdana"), textDraw.TextSize, Brushes.Black,
                          VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;
                Chart.ShowTextEdit(textDraw, viewPos, 55, textHeight + 6);
                Chart.ResetDrawMode();               
            }
            else
            {
                return false;
            }

            Render();
            return true; 
        }

        private bool CheckMoveOrder(Point point)
        {
            _moveOrderStart = false;
            var price = Chart.GetPriceFromPoint(point, Chart.ChartControl.MainPane);

            var orderDraw = Chart.MoveOrderDraw;
            if (price.HasValue && orderDraw != null)
            {
                var p = MyWpfMath.RoundToTickSize(price.Value, Chart.TickSize);
                Chart.OrderChanged?.Invoke(this, new OrderChangeEventArgs(orderDraw.Guid, null, p));
                Chart.MoveOrderDraw = null;
                Render();
                Chart.ChartControl.MainPane.valueTicksElement.Render();
                return true; 
            }
            return false;
        }

        private bool CheckOrderHit(Point point)
        {
            var orderCloseBoxes = _orderCloseBoxes.Keys.ToDictionary(x => x, x => _orderCloseBoxes[x]);
            foreach (var orderCloseBox in orderCloseBoxes)
            {
                if (orderCloseBox.Value.FillContains(point))
                {
                    Chart.OrderCanceled.Invoke(this, orderCloseBox.Key);
                    Cursor = Cursors.SizeAll;
                    return true;
                }
            }

            var orderQntBoxes = _orderQntBoxes.Keys.ToDictionary(x => x, x => _orderQntBoxes[x]);
            foreach (var orderQntBox in orderQntBoxes)
            {
                if (orderQntBox.Value.FillContains(point))
                {
                    var figure = orderQntBox.Value.Figures.FirstOrDefault();
                    if (figure != null)
                    {
                        var priceTickFontSize = 10.0;
                        double textHeight = (new FormattedText("123", CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight, new Typeface("Verdana"), priceTickFontSize, Brushes.Black,
                            VisualTreeHelper.GetDpi(this).PixelsPerDip)).Height;

                        Chart.ShowOrderQntEdit(orderQntBox.Key, figure.StartPoint, 55, textHeight + 6);                        
                        return true; 
                    }
                }
            }

            var orderMoveGeometries = _orderMoveGeometries.Keys.ToDictionary(x => x, x => _orderMoveGeometries[x]);
            foreach (var orderMoveGeometry in orderMoveGeometries)
            {
                if (orderMoveGeometry.Value.FillContains(point))
                {
                    var orderDraw = Chart.OrderDraws[orderMoveGeometry.Key];
                    Chart.MoveOrderDraw = (OrderDraw)orderDraw.Clone();
                    _moveOrderStart = true;
                    Render();
                    Chart.ChartControl.MainPane.valueTicksElement.Render();                    
                    Cursor = Cursors.SizeAll;
                    return true;
                }
            }
            return false;
        }


        private bool CheckDrawHit(Point curPoint, Point viewPosition)
        {
            var drawMoveGeometries = _drawMoveGeometries.Keys.ToDictionary(x => x, x => _drawMoveGeometries[x]);
            foreach (var drawMoveGeometry in drawMoveGeometries)
            {
                if (drawMoveGeometry.Value.FillContains(viewPosition))
                {
                    //Check line and vline
                    if (Chart.LineDraws.ContainsKey(drawMoveGeometry.Key))
                    {
                        var lineDraw = Chart.LineDraws[drawMoveGeometry.Key];                        
                        var curIndex = Chart.GetBarIndexFromPoint(curPoint);
                        double valueRange = VisibleValuesExtremums.CandleValueHigh - VisibleValuesExtremums.CandleValueLow;
                        var lowValue = VisibleValuesExtremums.CandleValueLow;
                        double startY = (1.0 - (lineDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                        double endY = (1.0 - (lineDraw.EndY - lowValue) / valueRange) * RenderSize.Height;
                        var curPointY = viewPosition.Y;
                        if (lineDraw is BaseLineDraw)
                        {
                            if (!lineDraw.IsHorizontal && !lineDraw.IsVertical)
                            {
                                if (((startY - adjustSizePoint <= curPointY && startY + adjustSizePoint >= curPointY) && lineDraw.BarIndexStart == curIndex)
                                    )
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustStart = true;
                                }
                                else if ((endY - adjustSizePoint <= curPointY && endY + adjustSizePoint >= curPointY) && lineDraw.BarIndexEnd == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustEnd = true;
                                }
                                else if ((endY - adjustSizePoint <= curPointY && endY + adjustSizePoint >= curPointY) && lineDraw.BarIndexStart == curIndex && lineDraw is RectDraw)
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustStart = true;
                                    lineDraw.AdjustPoint = true;
                                }
                                else if ((startY - adjustSizePoint <= curPointY && startY + adjustSizePoint >= curPointY) && lineDraw.BarIndexEnd == curIndex && lineDraw is RectDraw)
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustEnd = true;
                                    lineDraw.AdjustPoint = true;

                                }
                            }

                            if (lineDraw is RulerDraw rulerDraw)
                            {
                                double infoY = (1.0 - (rulerDraw.InfoY - lowValue) / valueRange) * RenderSize.Height;
                                if ((infoY - adjustSizePoint <= curPointY && infoY + adjustSizePoint >= curPointY) && rulerDraw.InfoBarIndex == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustPoint = true;
                                }
                            }
                            else if (lineDraw is RiskDraw riskDraw)
                            {
                                double rewwardY = (1.0 - (riskDraw.RewardY - lowValue) / valueRange) * RenderSize.Height;
                                if ((rewwardY - adjustSizePoint <= curPointY && rewwardY + adjustSizePoint >= curPointY) && riskDraw.RewardBarIndex == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                    Chart.AdjustDraw = lineDraw;
                                    lineDraw.IsAdjust = true;
                                    lineDraw.AdjustPoint = true;
                                }
                            }
                        }

                        if (!lineDraw.IsAdjust)
                        {
                            _startDrawMove = true;
                            _startMovePosition = curPoint;
                            Cursor = Cursors.SizeAll;
                        }

                        Chart.SelectedDraw = lineDraw;                        
                        return true;
                    }

                    //check hline
                    foreach (var paneHLineDraw in Chart.PaneHLineDraws)
                    {
                        if (paneHLineDraw.Value.ContainsKey(drawMoveGeometry.Key))
                        {
                            var lineDraw = paneHLineDraw.Value[drawMoveGeometry.Key];
                            _startDrawMove = true;
                            _startMovePosition = curPoint;
                            Chart.SelectedDraw = lineDraw;                            
                            Cursor = Cursors.SizeAll;
                            return true;
                        }
                    }

                    var textDraw = Chart.TextDraws.Values.FirstOrDefault(x => x.Tag == drawMoveGeometry.Key);
                    //Check text
                    if (textDraw != null)
                    {
                        _startDrawMove = true;
                        _startMovePosition = curPoint;
                        Chart.SelectedDraw = textDraw;                    
                        Cursor = Cursors.SizeAll;
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton != MouseButton.Left)
            {
                CancelMoveOrderDraw();
                CancelCurrentDraw();
                return;
            }

            var viewPosition = e.GetPosition(this);
            var currentHitPoint = e.GetPosition(Chart.ChartControl.MainPane);
            if (Chart.IsDrawingToolsVisible)
            {
              
                if (CheckStartEndDraw(currentHitPoint, viewPosition))
                    return;
            }

            if (_moveOrderStart)
            {
                if (CheckMoveOrder(currentHitPoint))
                    return;
            }
           

            if (!_moveOrderStart)
            {
                if (CheckOrderHit(viewPosition))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (CheckDrawHit(currentHitPoint, viewPosition))
            {
                e.Handled = true;
                return;
            }
            
            
            Chart.SelectedDraw = null;
        }

        public bool IsDrawing
        {
            get { return _startDrawObject; }
        }

        private bool _moveOrderStart;

        private int adjustSizePoint = 4;

        private bool DrawCurrentObject(double currentPrice, Point point)
        {
            if (!Chart.IsDrawingToolsVisible)
                return false;

            if (Chart.IsLineMode && Chart.CurrentDraw is LineDraw lineDraw)
            {
                lineDraw.EndY = currentPrice;
                lineDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
            }
            else if (Chart.IsRayMode && Chart.CurrentDraw is RayDraw rayDraw)
            {
                rayDraw.EndY = currentPrice;
                rayDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
            }
            else if (Chart.IsRectMode && Chart.CurrentDraw is RectDraw rectDraw)
            {
                rectDraw.EndY = currentPrice;
                rectDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
            }
            else if (Chart.IsRulerMode && Chart.CurrentDraw is RulerDraw rulerDraw)
            {
                if (!rulerDraw.IsRulerFixed)
                {
                    rulerDraw.EndY = currentPrice;
                    rulerDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
                }
                else
                {
                    rulerDraw.InfoY = currentPrice;
                    rulerDraw.InfoBarIndex = Chart.GetBarIndexFromPoint(point);
                }
            }
            else if (Chart.IsRiskMode && Chart.CurrentDraw is RiskDraw riskDraw)
            {
                riskDraw.EndY = currentPrice;
                riskDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
            }
            else
            {
                return false;
            }
            Render();
            return true;
        }


        private bool AdjustCurrentDraw(double price, Point point)
        {
            if (Chart.AdjustDraw is BaseLineDraw adjustLineDraw)
            {
                if (!adjustLineDraw.IsHorizontal && !adjustLineDraw.IsVertical)
                {
                    if (adjustLineDraw.IsAdjust)
                    {
                        if (adjustLineDraw.AdjustStart)
                        {
                            Chart.BarLines[adjustLineDraw.BarIndexStart].Remove(adjustLineDraw.Tag);
                            if (!Chart.BarLines[adjustLineDraw.BarIndexStart].Any())
                                Chart.BarLines.Remove(adjustLineDraw.BarIndexStart);

                            if (adjustLineDraw.AdjustPoint)
                            {
                                adjustLineDraw.EndY = price;
                                adjustLineDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);

                            }
                            else
                            {
                                adjustLineDraw.StartY = price;
                                adjustLineDraw.BarIndexStart = Chart.GetBarIndexFromPoint(point);
                            }

                            if (Chart.BarLines.ContainsKey(adjustLineDraw.BarIndexStart))
                            {
                                Chart.BarLines[adjustLineDraw.BarIndexStart].Add(adjustLineDraw.Tag);
                            }
                            else
                            {
                                Chart.BarLines.Add(adjustLineDraw.BarIndexStart, new List<string>() { adjustLineDraw.Tag });
                            }

                            if (adjustLineDraw is RiskDraw riskDraw)
                            {                                
                                riskDraw.ApplyRatio();
                            }
                        }
                        else if (adjustLineDraw.AdjustEnd)
                        {

                            Chart.BarLines[adjustLineDraw.BarIndexEnd].Remove(adjustLineDraw.Tag);
                            if (!Chart.BarLines[adjustLineDraw.BarIndexEnd].Any())
                                Chart.BarLines.Remove(adjustLineDraw.BarIndexEnd);

                            if (adjustLineDraw.AdjustPoint)
                            {
                                adjustLineDraw.StartY = price;
                                adjustLineDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
                            }
                            else
                            {
                                adjustLineDraw.EndY = price;
                                adjustLineDraw.BarIndexEnd = Chart.GetBarIndexFromPoint(point);
                            }

                            if (Chart.BarLines.ContainsKey(adjustLineDraw.BarIndexEnd))
                            {
                                Chart.BarLines[adjustLineDraw.BarIndexEnd].Add(adjustLineDraw.Tag);
                            }
                            else
                            {
                                Chart.BarLines.Add(adjustLineDraw.BarIndexEnd, new List<string>() { adjustLineDraw.Tag });
                            }


                            if (adjustLineDraw is RiskDraw riskDraw)
                            {
                                riskDraw.ApplyRatio();
                            }
                        }
                        else if (adjustLineDraw is RulerDraw rulerDraw && adjustLineDraw.AdjustPoint)
                        {
                            rulerDraw.InfoY = price;
                            rulerDraw.InfoBarIndex = Chart.GetBarIndexFromPoint(point);
                        }
                        else if (adjustLineDraw is RiskDraw riskDraw && adjustLineDraw.AdjustPoint)
                        {
                            riskDraw.RewardY = price;
                            riskDraw.RewardBarIndex = Chart.GetBarIndexFromPoint(point);
                            riskDraw.ApplyRatioInverse();
                        }
                        Render();
                        return true;
                    }
                }
            }
            return false;
        }


        private bool MoveCurrentDraw(double price, Point currentPosition)
        {
            if (Chart.SelectedDraw is BaseLineDraw lineDraw)
            {
                if (lineDraw.IsHorizontal)
                {
                    //Horizontal
                    lineDraw.StartY = price;
                    lineDraw.EndY = price;
                }
                else if (lineDraw.IsVertical)
                {
                    Chart.BarLines[lineDraw.BarIndexStart].Remove(lineDraw.Tag);
                    Chart.BarLines[lineDraw.BarIndexEnd].Remove(lineDraw.Tag);

                    //Vertical
                    lineDraw.BarIndexStart = Chart.GetBarIndexFromPoint(currentPosition);
                    lineDraw.BarIndexEnd = lineDraw.BarIndexStart;

                    if (Chart.BarLines.ContainsKey(lineDraw.BarIndexStart))
                    {
                        Chart.BarLines[lineDraw.BarIndexStart].Add(lineDraw.Tag);
                    }
                    else
                    {
                        Chart.BarLines.Add(lineDraw.BarIndexStart, new List<string>() { lineDraw.Tag });
                    }

                }
                else
                {                    
                    Chart.BarLines[lineDraw.BarIndexStart].Remove(lineDraw.Tag);
                    if (!Chart.BarLines[lineDraw.BarIndexStart].Any())
                        Chart.BarLines.Remove(lineDraw.BarIndexStart);

                    Chart.BarLines[lineDraw.BarIndexEnd].Remove(lineDraw.Tag);
                    if (!Chart.BarLines[lineDraw.BarIndexEnd].Any())
                        Chart.BarLines.Remove(lineDraw.BarIndexEnd);

                    var barIndex = Chart.GetBarIndexFromPoint(currentPosition);
                    var oldPrice = Chart.GetPriceFromPoint(_startMovePosition, Chart.ChartControl.MainPane);
                    var oldBarIndex = Chart.GetBarIndexFromPoint(_startMovePosition);

                    var deltaPrice = price - oldPrice;
                    var deltaBarIndex = barIndex - oldBarIndex;

                    lineDraw.StartY += (deltaPrice ?? 0);
                    lineDraw.EndY += (deltaPrice ?? 0);
                    lineDraw.BarIndexStart += deltaBarIndex;
                    lineDraw.BarIndexEnd += deltaBarIndex;

                    if (Chart.BarLines.ContainsKey(lineDraw.BarIndexStart))
                    {
                        Chart.BarLines[lineDraw.BarIndexStart].Add(lineDraw.Tag);
                    }
                    else
                    {
                        Chart.BarLines.Add(lineDraw.BarIndexStart, new List<string>() { lineDraw.Tag });
                    }

                    if (Chart.BarLines.ContainsKey(lineDraw.BarIndexEnd))
                    {
                        Chart.BarLines[lineDraw.BarIndexEnd].Add(lineDraw.Tag);
                    }
                    else
                    {
                        Chart.BarLines.Add(lineDraw.BarIndexEnd, new List<string>() { lineDraw.Tag });
                    }

                    _startMovePosition = currentPosition;
                    if (lineDraw is RulerDraw rulerDraw)
                    {
                        rulerDraw.InfoY += (deltaPrice ?? 0);
                        rulerDraw.InfoBarIndex += deltaBarIndex;
                    }
                    else if (lineDraw is RiskDraw riskDraw)
                    {
                        riskDraw.RewardY += (deltaPrice ?? 0);
                        riskDraw.RewardBarIndex += deltaBarIndex;
                    }
                }

                Render();
                Chart.ChartControl.MainPane.valueTicksElement.Render();
            }
            else if (Chart.SelectedDraw is TextDraw textDraw)
            {

                textDraw.Y = price;
                var paneId = Chart.ChartControl.MainPane.Id;

                if (!Chart.PaneTextDraws.TryGetValue(paneId, out var textDraws))
                {
                    textDraws = new Dictionary<int, List<string>>();
                    Chart.PaneTextDraws.Add(paneId, textDraws);
                }

                if (!Chart.DrawingTools.PaneTextDrawTags.TryGetValue(paneId, out var textDrawTags))
                {
                    textDrawTags = new Dictionary<string, DrawInfo>();
                    Chart.DrawingTools.PaneTextDrawTags.Add(paneId, textDrawTags);
                }

                List<string> idList;
                //Remove prev text by tag
                if (textDrawTags.ContainsKey(textDraw.Tag))
                {
                    var drawInfo = textDrawTags[textDraw.Tag];
                    idList = textDraws[drawInfo.BarIndex];
                    idList.Remove(drawInfo.Id);
                    textDrawTags.Remove(textDraw.Tag);
                }

                var barIndex = Chart.GetBarIndexFromPoint(currentPosition);
                if (!textDraws.TryGetValue(barIndex, out idList))
                {
                    idList = new List<string> { };
                    textDraws.Add(barIndex, idList);
                }
                idList.Add(textDraw.Id);

                textDrawTags.Add(textDraw.Tag, new DrawInfo() { BarIndex = barIndex, Id = textDraw.Id });

                Render();
            }
            Cursor = Cursors.SizeAll;
            return true;
        }


        private void CheckCanAdjust(Point position)
        {
            if (Chart.SelectedDraw != null && _drawMoveGeometries.TryGetValue(Chart.SelectedDraw.Tag, out var drawMoveGeometry))
            {                
                if (drawMoveGeometry.FillContains(position))
                {
                    if (Chart.SelectedDraw is BaseLineDraw lineDraw)
                    {
                        if (!lineDraw.IsHorizontal && !lineDraw.IsVertical)
                        {
                            var curPoint = position;
                            var curPointY = position.Y;
                            var curIndex = Chart.GetBarIndexFromPoint(curPoint);
                            double valueRange = VisibleValuesExtremums.CandleValueHigh - VisibleValuesExtremums.CandleValueLow;
                            var lowValue = VisibleValuesExtremums.CandleValueLow;
                            double startY = (1.0 - (lineDraw.StartY - lowValue) / valueRange) * RenderSize.Height;
                            double endY = (1.0 - (lineDraw.EndY - lowValue) / valueRange) * RenderSize.Height;

                            if (lineDraw is RectDraw)
                            {
                                if ((startY - adjustSizePoint <= curPointY && startY + adjustSizePoint >= curPointY) && lineDraw.BarIndexStart == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else if ((endY - adjustSizePoint <= curPointY && endY + adjustSizePoint >= curPointY) && lineDraw.BarIndexEnd == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else if ((endY + -adjustSizePoint <= curPointY && endY + adjustSizePoint >= curPointY) && lineDraw.BarIndexStart == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else if ((startY - adjustSizePoint <= curPointY && startY + adjustSizePoint >= curPointY) && lineDraw.BarIndexEnd == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else

                                    Cursor = Cursors.SizeAll;
                            }
                            else
                            {
                                if ((startY - adjustSizePoint <= curPointY && startY + adjustSizePoint >= curPointY) && lineDraw.BarIndexStart == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else if ((endY - adjustSizePoint <= curPointY && endY + adjustSizePoint >= curPointY) && lineDraw.BarIndexEnd == curIndex)
                                {
                                    Cursor = Cursors.SizeNESW;
                                }
                                else if (lineDraw is RulerDraw rulerDraw)
                                {
                                    double infoY = (1.0 - (rulerDraw.InfoY - lowValue) / valueRange) * RenderSize.Height;
                                    if ((infoY - adjustSizePoint <= curPointY && infoY + adjustSizePoint >= curPointY) && rulerDraw.InfoBarIndex == curIndex)
                                    {
                                        Cursor = Cursors.SizeNESW;
                                    }
                                    else
                                        Cursor = Cursors.SizeAll;
                                }
                                else if (lineDraw is RiskDraw riskDraw)
                                {
                                    double rewardY = (1.0 - (riskDraw.RewardY - lowValue) / valueRange) * RenderSize.Height;
                                    if ((rewardY - adjustSizePoint <= curPointY && rewardY + adjustSizePoint >= curPointY) && riskDraw.RewardBarIndex == curIndex)
                                    {
                                        Cursor = Cursors.SizeNESW;
                                    }
                                    else
                                        Cursor = Cursors.SizeAll;
                                }
                                else
                                    Cursor = Cursors.SizeAll;
                            }
                        }
                        else
                        {
                            Cursor = Cursors.SizeAll;
                        }
                    }
                }
                else
                    Cursor = Cursors.Arrow;
            }
            else
                Cursor = Cursors.Arrow;
        }

        private void ProcessMoveOrder(double price)
        {
            //Process move order
            if (_moveOrderStart)
            {
                var orderDraw = Chart.MoveOrderDraw;
                if (orderDraw != null && price != 0)
                {
                    orderDraw.LimitPrice = MyWpfMath.RoundToTickSize(price, Chart.TickSize);
                    orderDraw.StopPrice = MyWpfMath.RoundToTickSize(price, Chart.TickSize);
                    Render();
                    Chart.ChartControl.MainPane.valueTicksElement.Render();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var currentPoint = e.GetPosition(Chart.ChartControl.MainPane);
            var price = Chart.GetPriceFromPoint(currentPoint, Chart.ChartControl.MainPane) ?? 0;

            if (_startDrawObject)
            {
                if (DrawCurrentObject(price, currentPoint))
                    return;
            }

            if (AdjustCurrentDraw(price, currentPoint))
                return;
           
            if (_startDrawMove)
            {
                MoveCurrentDraw(price, currentPoint);
            }
            else
            {
                CheckCanAdjust(e.GetPosition(this));
            }


            ProcessMoveOrder(price);
        }

        public void RemoveOrder(string orderGuid)
        {
            _orderCloseBoxes.Remove(orderGuid);
            _orderQntBoxes.Remove(orderGuid);
            _orderMoveGeometries.Remove(orderGuid);
        }


        public class PointEqualityComparer : IEqualityComparer<Point>
        {
            public bool Equals(Point x, Point y)
            {
                return (x.X == y.X && x.Y == y.Y);
            }

            public int GetHashCode(Point obj)
            {
                return (obj.X.GetHashCode() + obj.Y.GetHashCode());
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------

    }
}