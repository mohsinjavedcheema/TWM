using System.Windows.Media;

namespace Twm.Chart.Classes
{
    public class Settings
    {

        ///<summary>Gets the default value for the TradeBuyArrowFill property.</summary>
        ///<value>The default value for the TradeBuyArrowFill property: <c>Brushes.Blue</c>.</value>
        ///<seealso cref = "TradeBuyArrowFill">TradeBuyArrowFill</seealso>
        public static Brush DefaultTradeBuyArrowFill { get { return (Brush)(new SolidColorBrush(Colors.Blue)).GetCurrentValueAsFrozen(); } }


        ///<summary>Gets the default value for the TradeSellArrowFill property.</summary>
        ///<value>The default value for the TradeSellArrowFill property: <c>Brushes.Magenta</c>.</value>
        ///<seealso cref = "TradeSellArrowFill">TradeSellArrowFill</seealso>
        public static Brush DefaultTradeSellArrowFill { get { return (Brush)(new SolidColorBrush(Colors.Magenta)).GetCurrentValueAsFrozen(); } }


        ///<summary>Gets the default value for the BullishCandleFill property.</summary>
        ///<value>The default value for the BullishCandleFill property: <c>Brushes.LimeGreen</c>.</value>
        ///<seealso cref = "BullishCandleFill">BullishCandleFill</seealso>
        public static Brush DefaultBullishCandleFill { get { return (Brush)(new SolidColorBrush(Colors.LimeGreen)).GetCurrentValueAsFrozen(); } }

        ///<summary>Gets the default value for the WickCandleFill property.</summary>
        ///<value>The default value for the WickCandleFill property: <c>Brushes.Black</c>.</value>
        ///<seealso cref = "WickCandleFill">WickCandleFill</seealso>
        public static Brush DefaultWickCandleFill { get { return (Brush)(new SolidColorBrush(Colors.Black)).GetCurrentValueAsFrozen(); } }


        ///<summary>Gets the default value for the BearishCandleFill property.</summary>
        ///<value>The default value for the BearishCandleFill property: <c>Brushes.Red</c>.</value>
        ///<seealso cref = "BearishCandleFill">BearishCandleFill</seealso>
        public static Brush DefaultBearishCandleFill { get { return (Brush)(new SolidColorBrush(Colors.Red)).GetCurrentValueAsFrozen(); } }

        ///<summary>Gets the default value for the Brush constituent of the HorizontalGridlinesPen property.</summary>
        ///<value>The default value for the <see cref="Brush"/> constituent of the <see cref="HorizontalGridlinesPen"/> property: <c>#1E000000</c>.</value>
        ///<seealso cref = "DefaultHorizontalGridlinesThickness">DefaultHorizontalGridlinesThickness</seealso>
        //public static Brush DefaultHorizontalGridlinesBrush { get { return (Brush)(new SolidColorBrush(Color.FromArgb(30, 0, 0, 0))).GetCurrentValueAsFrozen(); } } // #1E000000
        public static Brush DefaultHorizontalGridlinesBrush { get { return (Brush)(new SolidColorBrush(Colors.LightGray)).GetCurrentValueAsFrozen(); } } // #1E000000


        ///<summary>Gets the default value for Thickness constituent of the HorizontalGridlinesPen property.</summary>
        ///<value>The default value for the Thickness constituent of the <see cref="HorizontalGridlinesPen"/> property: <c>1.0</c>.</value>
        ///<seealso cref = "DefaultHorizontalGridlinesBrush">DefaultHorizontalGridlinesBrush</seealso>
        public static double DefaultHorizontalGridlinesThickness { get { return 1.0; } }


        ///<summary>Gets the default value for the Brush constituent of the VerticalGridlinesPen property.</summary>
        ///<value>The default value for the <see cref="Brush"/> constituent of the <see cref="VerticalGridlinesPen"/> property: <c>#1E000000</c>.</value>
        ///<seealso cref = "DefaultVerticalGridlinesThickness">DefaultVerticalGridlinesThickness</seealso>
        //public static Brush DefaultVerticalGridlinesBrush { get { return (Brush)(new SolidColorBrush(Color.FromArgb(50, 105, 42, 0))).GetCurrentValueAsFrozen(); } } // #32692A00
        public static Brush DefaultVerticalGridlinesBrush { get { return (Brush)(new SolidColorBrush(Colors.LightGray)).GetCurrentValueAsFrozen(); } } // #32692A00

        ///<summary>Gets the default value for Thickness constituent of the VerticalGridlinesPen property.</summary>
        ///<value>The default value for the Thickness constituent of the <see cref="VerticalGridlinesPen"/> property: <c>1.0</c>.</value>
        ///<seealso cref = "DefaultVerticalGridlinesBrush">DefaultVerticalGridlinesBrush</seealso>
        public static double DefaultVerticalGridlinesThickness { get { return 1.0; } }

        ///<summary>Gets the default value for the <see cref="TimeTickFontSize">TimeTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="TimeTickFontSize"/> property: <c>10.0</c>.</value>
        public static double DefaultTimeTickFontSize { get { return 10.0; } }


        ///<summary>Gets the default value for the <see cref="AxisTickColor">AxisTickColor</see> property.</summary>
        ///<value>The default value for the <see cref="AxisTickColor"/> property: <c>Brushes.Black</c>.</value>
        public static Brush DefaultAxisTickColor { get { return (Brush)(new SolidColorBrush(Colors.Black)).GetCurrentValueAsFrozen(); } }


        ///<summary>Gets the default value for the <see cref="PriceTickFontSize">PriceTickFontSize</see> property.</summary>
        ///<value>The default value for the <see cref="PriceTickFontSize"/> property: <c>11.0</c>.</value>
        public static double DefaultPriceTickFontSize { get { return 11.0; } }

        ///<summary>Gets the default value for the <see cref="GapBetweenPriceTickLabels">GapBetweenPriceTickLabels</see> property.</summary>
        ///<value>The default value for the <see cref="GapBetweenPriceTickLabels"/> property: <c>0.0</c>.</value>
        public static double DefaultGapBetweenPriceTickLabels { get { return 0.0; } }

    }
}