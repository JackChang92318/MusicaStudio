using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_v1
{
    public class measureCanvas : Canvas
    {
        public static double SourceWidth = frameCanvas.SourceWidth;
        public int MeasureCount { get; set; } = 8;
        public Brush LineBrush { get; set; } = Brushes.Gray;
        public Brush DarkBrush { get; set; } = Brushes.Black;
        public Brush TextBrush { get; set; } = Brushes.Black;
        public Typeface TextTypeface { get; set; } = new Typeface("Arial");
        public double TextFontSize { get; set; } = 16;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double width = this.ActualWidth;
            double height = this.ActualHeight;
            double requiredWidth = (int)(width / SourceWidth) * SourceWidth;

            Pen linePen = new Pen(LineBrush, 1);
            for (double x = SourceWidth; x <= requiredWidth; x += SourceWidth)
            {
                dc.DrawLine(linePen, new Point(x, 0), new Point(x, height));
            }

            for (double x = 1; x < requiredWidth; x += SourceWidth)
            {
                int measureIndex = (int)(x / SourceWidth);
                int measureNumber = (measureIndex / 16) + 1;

                FormattedText text = new FormattedText(
                    measureNumber.ToString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    TextTypeface,
                    TextFontSize,
                    TextBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                double textX = x + (SourceWidth - text.Width) / 2;
                double textY = (height - text.Height) / 2;
                dc.DrawText(text, new Point(textX, textY));
            }
        }

    }
}

