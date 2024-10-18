using System.Windows.Media;
using Twm.Chart.Enums;

namespace Twm.Chart.DrawObjects
{
    public class ArrowDraw
    {
        public string Id { get; set; }
        public ArrowDirection Direction { get; set; }
        public string Tag { get; set; }
        public double Y { get; set; }
        public ArrowConnector Connector { get; set; }
        public Brush Brush { get; set; }
    }
}