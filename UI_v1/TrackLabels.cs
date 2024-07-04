using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace UI_v1
{
    public class TrackLabel : Label
    {
        private double _sourceWidth;
        public double SourceWidth
        {
            get => _sourceWidth;
            set
            {
                _sourceWidth = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double sourceWidth = SourceWidth;
            double labelHeight = this.ActualHeight;

            if (SourceWidth > 0)
            {
                int numberOfLines = (int)(sourceWidth / SourceWidth);
                double spacing = SourceWidth;

                Pen pen = new Pen(Brushes.Black, 1);

                for (int i = 1; i <= numberOfLines; i++)
                {
                    double x = i * spacing;
                    drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, labelHeight));
                }
            }
        }
    }
}
