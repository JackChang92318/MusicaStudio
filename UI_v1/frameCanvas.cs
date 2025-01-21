using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_v1
{
    public class frameCanvas : Canvas
    {
        public static double SourceWidth = MainWindow.SourceWidth;

        public Brush LineBrush { get; set; } = Brushes.Gray;
        public Brush DarkBrush { get; set; } = Brushes.Black;
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
            Pen lastPen = new Pen(DarkBrush, 2);

            int lineCount = 0;

            for (double x = SourceWidth; x < requiredWidth; x += SourceWidth)
            {
                lineCount++;

                if (lineCount % 4 == 0)
                {
                    dc.DrawLine(lastPen, new Point(x, 0), new Point(x, height));  // 畫深色線
                }
                else
                {
                    dc.DrawLine(linePen, new Point(x, 0), new Point(x, height));  // 畫淺色線
                }
            }
        }
    }
}

