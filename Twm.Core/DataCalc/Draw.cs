using System;
using System.Collections.Generic;
using System.Windows.Media;
using Twm.Chart.Controls;
using Twm.Chart.DrawObjects;
using Twm.Chart.Enums;

namespace Twm.Core.DataCalc
{
    public class Draw : Twm.Chart.DrawingTools.Draw
    {
        public void Arrow(ScriptBase scriptBase, ArrowDirection direction, string tag, int barsAgo, double y,
            ArrowConnector connector, Brush brush, PaneControl pane = null)
        {
            Arrow(scriptBase.CurrentBar - 1 - barsAgo, direction, tag, barsAgo, y, connector, brush, pane);
        }


        public void Line(ScriptBase scriptBase, string tag, int startBarsAgo, double startY, int endBarsAgo,
            double endY, Brush brush, int width, DashStyle dash, bool isAutoScale = false)
        {
            int barStartIndex, barEndIndex;
            if (scriptBase != null)
            {
                barStartIndex = scriptBase.CurrentBar - 1 - startBarsAgo;
                barEndIndex = scriptBase.CurrentBar - 1 - endBarsAgo;
            }
            else
            {
                barStartIndex = startBarsAgo;
                barEndIndex = endBarsAgo;
            }
            Line(barStartIndex, barEndIndex, tag, startY, endY, brush, width, dash, isAutoScale);
        }




        public void LineVertical(string tag, int barIndex, Brush brush, int width, DashStyle dash)
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

            Chart.LineDraws.Add(tag, new LineDraw()
            {
                StartY = -1,
                EndY = -1,
                Brush = newBrush,
                Tag = tag,
                Width = width,
                DashStyle = dash,
                BarIndexStart = barIndex,
                BarIndexEnd = barIndex
            });

            if (Chart.BarLines.ContainsKey(barIndex))
            {
                Chart.BarLines[barIndex].Add(tag);
            }
            else
            {
                Chart.BarLines.Add(barIndex, new List<string>() { tag });
            }

        }


        public void Line(ScriptBase scriptBase, string tag, int barsAgo, double Y, Brush brush, int width, bool isAutoScale = false)
        {
            var barIndex = scriptBase.CurrentBar - 1 - barsAgo;
            Line(barIndex, tag, barsAgo, Y, brush, width, isAutoScale);
        }


        public void Text(ScriptBase scriptBase, string tag, string text, int barsAgo, double y, Brush textBrush = null,
            string fontName = "", int textSize = 12, Brush outlineBrush = null, Brush areaBrush = null,
            double areaOpacity = 1.0, bool yTop = true, PaneControl pane = null)
        {
            var barIndex = scriptBase.CurrentBar - 1 - barsAgo;
            Text(barIndex, tag,text,barsAgo, y, textBrush, fontName, textSize, outlineBrush, areaBrush, areaOpacity, yTop, pane);

        }


        public void Reset()
        {
            PaneArrowDrawTags.Clear();
            PaneTextDrawTags.Clear();
        }
    }


}