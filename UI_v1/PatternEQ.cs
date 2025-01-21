using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_v1
{
    // string: SampleData => sample name : below
    public class SampleData
    {
        public List<PatternEQ> Patterns { get; set; }
        public bool IsActive { get; set; } = true;
        public double Volume { get; set; } = 1.0;
        public object ComboBoxItemTag { get; set; } = null;
        public bool sideChainActive { get; set; } = false;
    }
    public class PatternEQ
    {
        public bool IsActive { get; set; } = false;
        public double Bass { get; set; } = 0.0;
        public double Mid { get; set; } = 0.0;
        public double Treble { get; set; } = 0.0;
        public int tone { get; set; } = 1;
    }
}
