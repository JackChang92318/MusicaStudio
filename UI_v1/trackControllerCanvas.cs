using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UI_v1
{
    public class trackControllerCanvas : Canvas
    {
        public MainWindow MainWindow { get; set; }
        public KnobControl SideChainKnob { get; set; }
        public KnobControl VolumeKnob { get; set; }
        public Label open { get; set; }

        public trackControllerCanvas(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            open = new Label()
            {
                Name = "SideChainSwitch",
                Background = new SolidColorBrush(Color.FromRgb(255, 112, 112)),
                Height = 20,
                Width = 20,
            };
            open.MouseLeftButtonDown += OpenSideChain;
            Children.Add(open);

            SetLeft(open, 0);
            SetTop(open, 50);

            VolumeKnob = new KnobControl(mainWindow)
            {
                Name = "VolumeKnob",
                Width = 50,
                Height = 50,
                Value = 0.5f,//TODO: color
            };
            Children.Add(VolumeKnob);

            SetLeft(VolumeKnob, ActualWidth / 4 - VolumeKnob.Width / 2);
            SetTop(VolumeKnob, ActualHeight / 2 - VolumeKnob.Height / 2);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //SetLeft(SideChainKnob, ActualWidth / 4 - SideChainKnob.Width / 2);
            //SetTop(SideChainKnob, ActualHeight / 2 - SideChainKnob.Height / 2);

            SetLeft(open, 0);
            SetTop(open, 0);

            SetLeft(VolumeKnob, 3 * ActualWidth / 4 - VolumeKnob.Width / 2);
            SetTop(VolumeKnob, ActualHeight / 2 - VolumeKnob.Height / 2);
        }

        private void OpenSideChain(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                var track = VisualTreeHelper.GetParent(label) as trackControllerCanvas;
                var position = e.GetPosition(track);
                int y = Int32.Parse(track.Name.Substring(10));//track number

                string sampleName = MainWindow.TrackNumberToSampleName[y.ToString()];//System.Collections.Generic.KeyNotFoundException: '指定的索引鍵不在字典中。'
                var pattern = MainWindow.patternDictionary[sampleName];
                pattern.sideChainActive = !pattern.sideChainActive;

                if (pattern.sideChainActive)
                {
                    label.Background = new SolidColorBrush(Color.FromRgb(112, 255, 112));
                    Debug.WriteLine("Open SideChain!");
                }
                else
                {
                    label.Background = new SolidColorBrush(Color.FromRgb(255, 112, 112));
                    Debug.WriteLine("Close SideChain!");
                }
            }
        }
    }
}
