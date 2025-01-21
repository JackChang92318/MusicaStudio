using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_v1
{
    public class sampleLabel : Label
    {
        public double Bass { get; set; } = 0.0;
        public double Mid { get; set; } = 0.0;
        public double Treble { get; set; } = 0.0;
        public int tone { get; set; } = 1;
        public int currTrack = 0;
        public int currMeasure = 0;
        public double CornerRadius { get; set; } = 10.0;
    }
}
