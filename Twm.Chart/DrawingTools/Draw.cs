using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Twm.Chart.Controls;
using Twm.Chart.DrawObjects;
using Twm.Chart.Enums;

namespace Twm.Chart.DrawingTools
{
    public class Draw
    {

        private string[] linePropertyNames = new string[] { "Width", "Brush", "DashStyle", "Name", "Ratio" };

        private string[] textPropertyNames = new string[] { "Text", "TextBrush", "AreaBrush", "FontName", "OutlineBrush", "AreaOpacity", "TextSize", "Name" };
        
        private string[] rectPropertyNames = new string[] {  "AreaBrush",  "OutlineBrush", "AreaOpacity", "Width", "DashStyle", "Name" };

        protected Chart.Classes.Chart Chart;

        public IDictionary<string, IDictionary<string, DrawInfo>> PaneArrowDrawTags { get; set; } =
            new Dictionary<string, IDictionary<string, DrawInfo>>();

        public IDictionary<string, IDictionary<string, DrawInfo>> PaneTextDrawTags { get; set; } =
            new Dictionary<string, IDictionary<string, DrawInfo>>();



        private int _horizontalLineSeq;
        private int _verticalLineSeq;
        private int _lineSeq;
        private int _textSeq;
        private int _rectSeq;
        private int _raySeq;
        private int _rulerSeq;
        private int _riskSeq;

        public void SetChart(Chart.Classes.Chart chart)
        {

            Chart = chart;
        }

        public void Arrow(int barIndex, ArrowDirection direction, string tag, int barsAgo, double y,
            ArrowConnector connector, Brush brush, PaneControl pane = null)
        {
            if (Chart == null)
                return;


            if (barIndex < 0)
                throw new Exception("BarIndex out of range: " + barIndex);


            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();


            var paneId = Chart.ChartControl.MainPane.Id;
            if (pane != null)
            {
                paneId = pane.Id;
            }

            if (!Chart.PaneArrowDraws.TryGetValue(paneId, out var arrowDraws))
            {
                arrowDraws = new Dictionary<int, List<string>>();
                Chart.PaneArrowDraws.Add(paneId, arrowDraws);
            }

            if (!PaneArrowDrawTags.TryGetValue(paneId, out var arrowDrawTags))
            {
                arrowDrawTags = new Dictionary<string, DrawInfo>();
                PaneArrowDrawTags.Add(paneId, arrowDrawTags);
            }

            List<string> idList;
            //Remove prev text by tag
            if (arrowDrawTags.ContainsKey(tag))
            {
                var drawInfo = arrowDrawTags[tag];
                idList = arrowDraws[drawInfo.BarIndex];
                idList.Remove(drawInfo.Id);
                arrowDrawTags.Remove(tag);
            }


            //Add new text
            var id = Guid.NewGuid().ToString();
            Chart.ArrowDraws.Add(id, new ArrowDraw()
            {
                Id = id,
                Direction = direction,
                Tag = tag,
                Y = y,
                Connector = connector,
                Brush = newBrush
            });

            if (!arrowDraws.TryGetValue(barIndex, out idList))
            {
                idList = new List<string>();
                arrowDraws.Add(barIndex, idList);
            }
            idList.Add(id);

            arrowDrawTags.Add(tag, new DrawInfo() { BarIndex = barIndex, Id = id });

        }


        public void Line(int barStartIndex, int barEndIndex, string tag, double startY,
            double endY, Brush brush, int width, DashStyle dash, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barStartIndex < 0)
                throw new Exception("Bar start index out of range: " + barStartIndex);

            if (barEndIndex < 0)
                throw new Exception("Bar end index out of range: " + barEndIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var lineDraw = new LineDraw()
            {
                StartY = startY,
                EndY = endY,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barStartIndex,
                BarIndexEnd = barEndIndex,
                IsAutoScale = isAutoScale,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false
            };
            
            Chart.LineDraws.Add(tag, lineDraw);


            if (isDrawingObject)
            {
                Chart.SelectedDraw = lineDraw;
                lineDraw.Name = GetDrawName(DrawType.Line);
                Chart.DrawObjects.Add(lineDraw);
                lineDraw.PropertyChanged += LineDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barStartIndex))
            {
                Chart.BarLines[barStartIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barStartIndex, new List<string>() { tag });
            }

            if (Chart.BarLines.ContainsKey(barEndIndex))
            {
                Chart.BarLines[barEndIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barEndIndex, new List<string> { tag });
            }
        }

        private void LineDraw_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            if (linePropertyNames.Contains(e.PropertyName ))
            {
                if (e.PropertyName == "Ratio" )
                {
                    if (sender is RiskDraw riskDraw)
                    {
                        riskDraw.ApplyRatio();
                    }
                }

                Chart.RefreshPaneChart();
            }
                
        }

        public void LineHorizontal(string tag, double y, Brush brush, int width, DashStyle dash, PaneControl pane = null, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            var barIndex = -1;

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();


            var paneId = Chart.ChartControl.MainPane.Id;
            if (pane != null)
            {
                paneId = pane.Id;
            }

            if (!Chart.PaneHLineDraws.TryGetValue(paneId, out var lineDraws))
            {
                lineDraws = new Dictionary<string, LineDraw>();
                Chart.PaneHLineDraws.Add(paneId, lineDraws);
            }


            if (lineDraws.ContainsKey(tag))
            {
                lineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var lineDraw = new LineDraw()
            {
                StartY = y,
                EndY = y,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barIndex,
                BarIndexEnd = barIndex,
                IsAutoScale = isAutoScale,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false,
                IsHorizontal = true

            };

            lineDraws.Add(tag, lineDraw);

            if (isDrawingObject)
            {
                Chart.SelectedDraw = lineDraw;
                lineDraw.Name = GetDrawName(DrawType.HorizontalLine);
                Chart.DrawObjects.Add(lineDraw);
                lineDraw.PropertyChanged += LineDraw_PropertyChanged;
            }



        }

        public void LineVertical(string tag, int barIndex, Brush brush, int width, DashStyle dash, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barIndex < 0)
                throw new Exception("Bar index out of range: " + barIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var lineDraw = new LineDraw()
            {
                StartY = -1,
                EndY = -1,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barIndex,
                BarIndexEnd = barIndex,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false,
                IsVertical = true
            };

            Chart.LineDraws.Add(tag, lineDraw);

            if (isDrawingObject)
            {
                Chart.SelectedDraw = lineDraw;
                lineDraw.Name = GetDrawName(DrawType.VerticalLine);
                Chart.DrawObjects.Add(lineDraw);
                lineDraw.PropertyChanged += LineDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barIndex))
            {
                Chart.BarLines[barIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barIndex, new List<string>() { tag });
            }

        }


        public void Line(int barIndex, string tag, int barsAgo, double Y, Brush brush, int width, bool isAutoScale = false)
        {
            if (Chart == null)
                return;


            if (barIndex < 0)
                throw new Exception("Bar index out of range: " + barIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }


            Chart.LineDraws.Add(tag, new LineDraw()
            {
                StartY = Y,
                EndY = Y,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = DashStyles.Solid,
                BarIndexStart = barIndex,
                BarIndexEnd = barIndex,
                IsAutoScale = isAutoScale
            }); ;

            if (Chart.BarLines.ContainsKey(barIndex))
            {
                Chart.BarLines[barIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barIndex, new List<string>() { tag });
            }
        }


        public void Text(int barIndex, string tag, string text, int barsAgo, double y, Brush textBrush = null,
            string fontName = "", int textSize = 12, Brush outlineBrush = null, Brush areaBrush = null,
            double areaOpacity = 1.0, bool yTop = true, PaneControl pane = null, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;


            if (string.IsNullOrEmpty(fontName))
                fontName = "Veranda";

            if (textBrush == null)
                outlineBrush = Brushes.Black;

            if (outlineBrush == null)
                outlineBrush = Brushes.Transparent;

            if (areaBrush == null)
                areaBrush = Brushes.Transparent;

            if (barIndex < 0)
                throw new Exception("Bar start index out of range: " + barIndex);


            textBrush = textBrush.IsFrozen ? textBrush : (Brush)textBrush.GetCurrentValueAsFrozen();
            outlineBrush = outlineBrush.IsFrozen ? outlineBrush : (Brush)outlineBrush.GetCurrentValueAsFrozen();
            areaBrush = areaBrush.IsFrozen ? areaBrush : (Brush)areaBrush.GetCurrentValueAsFrozen();

            var paneId = Chart.ChartControl.MainPane.Id;
            if (pane != null)
            {
                paneId = pane.Id;
            }

            if (!Chart.PaneTextDraws.TryGetValue(paneId, out var textDraws))
            {
                textDraws = new Dictionary<int, List<string>>();
                Chart.PaneTextDraws.Add(paneId, textDraws);
            }

            if (!PaneTextDrawTags.TryGetValue(paneId, out var textDrawTags))
            {
                textDrawTags = new Dictionary<string, DrawInfo>();
                PaneTextDrawTags.Add(paneId, textDrawTags);
            }

            List<string> idList;
            //Remove prev text by tag
            if (textDrawTags.ContainsKey(tag))
            {
                var drawInfo = textDrawTags[tag];
                idList = textDraws[drawInfo.BarIndex];
                idList.Remove(drawInfo.Id);
                textDrawTags.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            //Add new text
            var id = Guid.NewGuid().ToString();
            var textDraw = new TextDraw()
            {
                Id = id,
                Y = y,
                TextBrush = textBrush,
                Text = text,
                Tag = tag,
                FontName = fontName,
                TextSize = textSize,
                OutlineBrush = outlineBrush,
                AreaBrush = areaBrush,
                AreaOpacity = areaOpacity,
                YTop = yTop,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false
            };
            Chart.TextDraws.Add(id, textDraw);

            if (!textDraws.TryGetValue(barIndex, out idList))
            {
                idList = new List<string> { };
                textDraws.Add(barIndex, idList);
            }
            idList.Add(id);

            textDrawTags.Add(tag, new DrawInfo() { BarIndex = barIndex, Id = id });

            if (isDrawingObject)
            {
                Chart.SelectedDraw = textDraw;
                textDraw.Name = GetDrawName(DrawType.Text);
                Chart.DrawObjects.Add(textDraw);
                textDraw.PropertyChanged += TextDraw_PropertyChanged;
            }

        }

        private void TextDraw_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (textPropertyNames.Contains(e.PropertyName))
                Chart.RefreshPaneChart();
        }



        public void Ray(int barStartIndex, int barEndIndex, string tag, double startY,
           double endY, Brush brush, int width, DashStyle dash, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barStartIndex < 0)
                throw new Exception("Bar start index out of range: " + barStartIndex);

            if (barEndIndex < 0)
                throw new Exception("Bar end index out of range: " + barEndIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var rayDraw = new RayDraw()
            {
                StartY = startY,
                EndY = endY,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barStartIndex,
                BarIndexEnd = barEndIndex,
                IsAutoScale = isAutoScale,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false
            };

            Chart.LineDraws.Add(tag, rayDraw);


            if (isDrawingObject)
            {
                Chart.SelectedDraw = rayDraw;
                rayDraw.Name = GetDrawName(DrawType.Ray);
                Chart.DrawObjects.Add(rayDraw);
                rayDraw.PropertyChanged += LineDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barStartIndex))
            {
                Chart.BarLines[barStartIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barStartIndex, new List<string>() { tag });
            }

            if (Chart.BarLines.ContainsKey(barEndIndex))
            {
                Chart.BarLines[barEndIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barEndIndex, new List<string> { tag });
            }
        }

        public void Rect(int barStartIndex, int barEndIndex, string tag, double startY,
            double endY, int width, DashStyle dash, Brush outlineBrush = null, Brush areaBrush = null,
            double areaOpacity = 1.0, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barStartIndex < 0)
                throw new Exception("Bar start index out of range: " + barStartIndex);

            if (barEndIndex < 0)
                throw new Exception("Bar end index out of range: " + barEndIndex);

            var outlineBrushFix = outlineBrush.IsFrozen ? outlineBrush : (Brush)outlineBrush.GetCurrentValueAsFrozen();
            var areaBrushFix = areaBrush.IsFrozen ? areaBrush : (Brush)areaBrush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var rectDraw = new RectDraw()
            {
                StartY = startY,
                EndY = endY,
                AreaBrush = areaBrush,
                OutlineBrush = outlineBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barStartIndex,
                BarIndexEnd = barEndIndex,                
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false
            };

            Chart.LineDraws.Add(tag, rectDraw);


            if (isDrawingObject)
            {
                Chart.SelectedDraw = rectDraw;
                rectDraw.Name = GetDrawName(DrawType.Rectangle);
                Chart.DrawObjects.Add(rectDraw);
                rectDraw.PropertyChanged += RectDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barStartIndex))
            {
                Chart.BarLines[barStartIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barStartIndex, new List<string>() { tag });
            }

            if (Chart.BarLines.ContainsKey(barEndIndex))
            {
                Chart.BarLines[barEndIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barEndIndex, new List<string> { tag });
            }
        }



        public void Ruler(int barStartIndex, int barEndIndex, int barInfoIndex, string tag, double startY,
            double endY, double infoY, Brush brush, int width, DashStyle dash, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barStartIndex < 0)
                throw new Exception("Bar start index out of range: " + barStartIndex);

            if (barEndIndex < 0)
                throw new Exception("Bar end index out of range: " + barEndIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var rulerDraw = new RulerDraw()
            {
                StartY = startY,
                EndY = endY,
                InfoY = infoY,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barStartIndex,
                BarIndexEnd = barEndIndex,
                InfoBarIndex = barInfoIndex,
                IsAutoScale = isAutoScale,
                IsDrawingObject = isDrawingObject,
                IsSelected = isDrawingObject ? true : false
            };

            Chart.LineDraws.Add(tag, rulerDraw);


            if (isDrawingObject)
            {
                Chart.SelectedDraw = rulerDraw;
                rulerDraw.Name = GetDrawName(DrawType.Ruler);
                Chart.DrawObjects.Add(rulerDraw);
                rulerDraw.PropertyChanged += LineDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barStartIndex))
            {
                Chart.BarLines[barStartIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barStartIndex, new List<string>() { tag });
            }

            if (Chart.BarLines.ContainsKey(barEndIndex))
            {
                Chart.BarLines[barEndIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barEndIndex, new List<string> { tag });
            }
        }



        public void Risk(int barStartIndex, int barEndIndex, int barRewardIndex, string tag, double startY,
            double endY, double rewardY, Brush brush, int width, DashStyle dash, double ratio, bool isAutoScale = false, bool isDrawingObject = false)
        {
            if (Chart == null)
                return;

            if (barStartIndex < 0)
                throw new Exception("Bar start index out of range: " + barStartIndex);

            if (barEndIndex < 0)
                throw new Exception("Bar end index out of range: " + barEndIndex);

            var newBrush = brush.IsFrozen ? brush : (Brush)brush.GetCurrentValueAsFrozen();

            if (Chart.LineDraws.ContainsKey(tag))
            {
                var oldLine = Chart.LineDraws[tag];
                Chart.BarLines[oldLine.BarIndexStart].Remove(tag);
                Chart.BarLines[oldLine.BarIndexEnd].Remove(tag);
                Chart.LineDraws.Remove(tag);
            }

            if (isDrawingObject && Chart.SelectedDraw != null)
            {
                Chart.SelectedDraw.IsSelected = false;
            }

            var riskDraw = new RiskDraw()
            {
                StartY = startY,
                EndY = endY,
                RewardY = rewardY,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barStartIndex,
                BarIndexEnd = barEndIndex,
                RewardBarIndex = barRewardIndex,
                IsAutoScale = isAutoScale,
                IsDrawingObject = isDrawingObject,
                Ratio = ratio,
                IsSelected = isDrawingObject ? true : false
            };

            Chart.LineDraws.Add(tag, riskDraw);


            if (isDrawingObject)
            {
                Chart.SelectedDraw = riskDraw;
                riskDraw.Name = GetDrawName(DrawType.RiskReward);
                Chart.DrawObjects.Add(riskDraw);
                riskDraw.PropertyChanged += LineDraw_PropertyChanged;
            }

            if (Chart.BarLines.ContainsKey(barStartIndex))
            {
                Chart.BarLines[barStartIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barStartIndex, new List<string>() { tag });
            }

            if (Chart.BarLines.ContainsKey(barEndIndex))
            {
                Chart.BarLines[barEndIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barEndIndex, new List<string> { tag });
            }
        }

        private void RectDraw_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (rectPropertyNames.Contains(e.PropertyName))
                Chart.RefreshPaneChart();
        }

        public void Reset()
        {
            PaneArrowDrawTags.Clear();
            PaneTextDrawTags.Clear();
        }

        private string GetDrawName(DrawType drawType)
        {
            var value = 0;
            var name = "";
            switch (drawType)
            {
                case DrawType.Line:
                    _lineSeq++;
                    value = _lineSeq;
                    name = "Line";
                    break;
                case DrawType.VerticalLine:
                    _verticalLineSeq++;
                    value = _verticalLineSeq;
                    name = "Vertical Line";
                    break;
                case DrawType.HorizontalLine:
                    _horizontalLineSeq++;
                    value = _horizontalLineSeq;
                    name = "Horizontal Line";
                    break;
                case DrawType.Ray:
                    _raySeq++;
                    value = _raySeq;
                    name = "Ray";
                    break;
                case DrawType.Text:
                    _textSeq++;
                    value = _textSeq;
                    name = "Text";
                    break;
                case DrawType.Rectangle:
                    _rectSeq++;
                    value = _rectSeq;
                    name = "Rectangle";
                    break;
                case DrawType.Ruler:
                    _rulerSeq++;
                    value = _rulerSeq;
                    name = "Ruler";
                    break;

                case DrawType.RiskReward:
                    _riskSeq++;
                    value = _riskSeq;
                    name = "RiskReward";
                    break;
            }

            if (value == 1)
                return name;

            return name+" " + value;
        }

    }
}
