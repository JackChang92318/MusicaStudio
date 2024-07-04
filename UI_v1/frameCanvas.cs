using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_v1
{
    public class frameCanvas:Canvas
    {
        public static double SourceWidth = MainWindow.SourceWidth;
        public Brush LineBrush { get; set; } = Brushes.Gray;
        public Brush BorderBrush { get; set; } = Brushes.Gray;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double width = this.ActualWidth;
            double height = this.ActualHeight;

            double requiredWidth = (int)(width / SourceWidth) * SourceWidth;

            Pen borderPen = new Pen(BorderBrush, 1);
            dc.DrawRectangle(null, borderPen, new Rect(0, 0, width, height));

            Pen linePen = new Pen(LineBrush, 1);
            for (double x = SourceWidth; x < requiredWidth; x += SourceWidth)
            {
                dc.DrawLine(linePen, new Point(x, 0), new Point(x, height));
            }
        }
    }
}
