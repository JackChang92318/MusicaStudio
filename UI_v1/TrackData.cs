using System.Collections.Generic;

namespace UI_v1
{
    public class TrackData
    {
        public bool IsActive { get; set; } = false;
        public double Volume { get; set; } = 1.0;
        public object ComboBoxItemTag { get; set; } = null;
        public bool sideChainActive { get; set; } = false;
        public Dictionary<string, SampleData> SampleDictionary { get; set; }
    }
}