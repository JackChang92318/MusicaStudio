using System;
using System.Diagnostics;
using System.Runtime.Remoting.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UI_v1
{
    public partial class MainWindow : Window
    {
        private bool _isInitialized = false;

        int octave = 0;
        int curSourceNum = 0;
        int curTracksNum = 3;


        public static double SourceWidth = 0;
        public static double SourceHeight = 0;

        private Point _MousePoint;
        private double _horizontalOffset;
        private double _verticalOffset;

        double _tracksHeight = 0;
        int trackNumber = 3;
        double currTrackWidth;

        private Label _draggedShadowLabel;
        private Label _copyLabel = new Label();

        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += MainWindow_SizeChanged;
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitialized = true;
            SetPianoRow();
            SetSources();
            SetTracks();
            SetCircleBtn();
        }
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized) return;
            SetPianoRow();
        }
        public void SetPianoRow()
        {
            keyContainer.Children.Clear();
            keyContainer.ColumnDefinitions.Clear();

            double actualWidth = piano_row.ColumnDefinitions[0].ActualWidth;
            double keyWidth = actualWidth / 36;

            for (int i = 0; i < 36; i++)
            {
                keyContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 1; i <= 36; i++)
            {
                Label label = new Label
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.Black),

                };// 2 4 7 9 11 black

                if(i % 12 == 2 || i % 12 == 4 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11)
                {
                    label.Background = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    label.Background = new SolidColorBrush(Colors.White);
                }

                Grid.SetColumn(label, i-1);

                Grid.SetRow(label, 0);

                keyContainer.Children.Add(label);
            }

            
            Label up = new Label
            {
                Name = "octaveUp",
                Content = "up",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.LightGray)
            };
            Label down = new Label
            {
                Name = "octaveDown",
                Content = "down",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Gray)
            };
            up.MouseLeftButtonDown += OctaveUp;
            down.MouseLeftButtonDown += OctaveDown;

            Grid.SetColumn(up, 1);
            Grid.SetRow(up, 0);

            Grid.SetColumn(down, 1);
            Grid.SetRow(down, 1);

            octaveBtn.Children.Add(up);
            octaveBtn.Children.Add(down);
        }
        public void OctaveUp(object sender, MouseButtonEventArgs e)
        {
            octave++;
            Console.WriteLine(octave);
        }
        public void OctaveDown(object sender, MouseButtonEventArgs e)
        {
            octave--;
            Console.WriteLine(octave);
        }
        public void SetSources()
        {
            //control source
            sourceContainer.Children.Clear();
            sourceContainer.ColumnDefinitions.Clear();

            double actualWidth = source.ColumnDefinitions[0].ActualWidth;
            double actualHeight = source.ActualHeight;
            SourceWidth = (actualWidth / 18);
            SourceHeight = actualHeight;

            for (int i = 0; i < 18; i++)
            {
                sourceContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });
            }

            for (int i = 1; i <= 18; i++)
            {
                Random r = new Random();
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                                  (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
                Label label = new Label
                {
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    Background = brush,
                    Margin = new Thickness(1),
                    Content = ""
                };
                //Set Drag Source Event
                label.MouseLeftButtonDown += Source_MouseDown;

                Grid.SetColumn(label, i - 1);
                Grid.SetRow(label, 0);
                
                curSourceNum += 1;
                sourceContainer.Children.Add(label);
            }

            // control scroll bar
            sourceScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sourceScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            sourceScroll.PreviewMouseRightButtonDown += SourceViewDrag_down;
            sourceScroll.PreviewMouseMove += SourceViewDrag_Move;
            sourceScroll.PreviewMouseRightButtonUp += SourceViewDrag_Up;
        }
        //Drag Source event
        private Label CopyLabel(Label sourceLabel)
        {
            if (sourceLabel == null) return null;

            Label newLabel = new Label
            {
                Content = sourceLabel.Content,
                FontSize = sourceLabel.FontSize,
                FontFamily = sourceLabel.FontFamily,
                FontWeight = sourceLabel.FontWeight,
                FontStyle = sourceLabel.FontStyle,
                Foreground = sourceLabel.Foreground,
                Background = sourceLabel.Background,
                BorderBrush = sourceLabel.BorderBrush,
                BorderThickness = sourceLabel.BorderThickness,
                Padding = sourceLabel.Padding,
                Margin = sourceLabel.Margin,
                HorizontalAlignment = sourceLabel.HorizontalAlignment,
                VerticalAlignment = sourceLabel.VerticalAlignment,
                Width = sourceLabel.Width,
                Height = sourceLabel.Height,
            };

            return newLabel;
        }
        private void Source_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _copyLabel = CopyLabel(sender as Label);
            CreateShadowLabel(_copyLabel, e);
            DragDrop.DoDragDrop(_copyLabel, _copyLabel.Content, DragDropEffects.Copy);
        }
        private void CreateShadowLabel(Label sourceLabel, MouseButtonEventArgs e)
        {
            Debug.WriteLine("CreateShadowLabel");

            _draggedShadowLabel = new Label
            {
                Content = sourceLabel.Content,
                FontSize = sourceLabel.FontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Gray),
                RenderTransform = new TranslateTransform(),
                Height = SourceHeight,
                Width = SourceWidth,
            };
            if(e.Source is frameCanvas track)
            {
                Debug.WriteLine("Adding to track");
                //track.Children.Add(_draggedShadowLabel);
            }
            else
            {
                Debug.WriteLine("Adding to other track");
                Grid.SetRow(_draggedShadowLabel, 1);
                //trackContainer.Children.Add(_draggedShadowLabel);
            }

            Canvas.SetTop(_draggedShadowLabel, Canvas.GetTop(sourceLabel));
            Canvas.SetLeft(_draggedShadowLabel, Canvas.GetLeft(sourceLabel));
        }
        private void Target_DragOver(object sender, DragEventArgs e)
        {
            var position = e.GetPosition(trackContainer);
            var transform = (TranslateTransform)_draggedShadowLabel.RenderTransform;
            Debug.WriteLine("mouse: " + position);
            Debug.WriteLine("SourceWidth: " + SourceWidth);

            transform.X = position.X - 12*(SourceWidth);
            transform.Y = position.Y - (SourceHeight/2);
            Debug.WriteLine("transform: " + transform.X);

        }
        private void Target_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is frameCanvas track)
            {
                Label newLabel = new Label
                {
                    Content = _copyLabel.Content,
                    Height = SourceHeight,
                    Width = SourceWidth,
                    Background = _copyLabel.Background
                };
                newLabel.MouseRightButtonDown += DeleteSource_MouseDown;

                var position = e.GetPosition(track);
                double x = Convert.ToInt16(Math.Floor(position.X / SourceWidth))* SourceWidth;
                double y = Convert.ToInt16(Math.Floor(position.Y / SourceHeight)) * SourceHeight;
                Canvas.SetLeft(newLabel, x);
                Canvas.SetTop(newLabel, y);

                track.Children.Add(newLabel);
                Debug.WriteLine("Target_Drop");
            }
        }
        private void SetCircleBtn()
        {
            double length = addSource.RowDefinitions[1].ActualHeight;

            Label addS = new Label
            {
                Width = length,
                Height = length,
                Background = Brushes.Transparent
            };
            Label addT = new Label
            {
                Width = length,
                Height = length,
                Background = Brushes.Transparent
            };

            Ellipse ellipse = new Ellipse
            {
                Fill = Brushes.Blue,
                Width = length,
                Height = length
            };

            ControlTemplate template = new ControlTemplate(typeof(Label));
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Ellipse));
            factory.SetValue(Ellipse.FillProperty, Brushes.LightGray);
            factory.SetValue(Ellipse.WidthProperty, length);
            factory.SetValue(Ellipse.HeightProperty, length);
            template.VisualTree = factory;
            addS.Template = template;
            addT.Template = template;

            addS.MouseLeftButtonDown += Addsource_MouseDown;
            addT.MouseLeftButtonDown += Addtrack_MouseDown;

            addSource.Children.Add(addS);
            addSource.Children.Add(addT);
            Grid.SetRow(addS, 0);
            Grid.SetColumn(addS, 0);
            Grid.SetRow(addT, 1);
            Grid.SetColumn(addT, 0);
        }
        public void SetTracks()
        {
            trackContainer.HorizontalAlignment = HorizontalAlignment.Left;
            //trackContainer.VerticalAlignment = VerticalAlignment.Center;

            _tracksHeight = source.ActualHeight;

            for(int i = 0; i < 3; i++)
            {
                trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
                trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

                frameCanvas canvas = new frameCanvas
                {
                    Width = SourceWidth,
                    Height = _tracksHeight,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(1),
                    AllowDrop = true
                };
                canvas.Loaded += (s, e) =>
                {
                    canvas.Width = SourceWidth;
                };

                Grid.SetColumn(canvas, 0);
                Grid.SetRow(canvas, 2*i+1);

                trackContainer.Children.Add(canvas);
            }

            //controll scroll bar
            //trackContainerScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            trackContainerScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            trackContainerScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            //Debug.WriteLine("Going to set click event");

            trackContainerScroll.ScrollChanged += Track_ScrollChange;
            trackContainerScroll.PreviewMouseRightButtonDown += TrackViewDrag_down;
            trackContainerScroll.PreviewMouseMove += TrackViewDrag_Move;
            trackContainerScroll.PreviewMouseRightButtonUp += TrackViewDrag_Up;
            trackContainerScroll.DragOver += Target_DragOver;
            trackContainerScroll.Drop += Target_Drop;

            //Debug.WriteLine("Set click event");

            trackControllerScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            trackControllerScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }
        public void Addsource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Active source mouse down event");
            sourceContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });

            Random r = new Random();
            Brush brush = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                              (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
            Label label = new Label
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Black),
                Background = brush,
                Margin = new Thickness(1),
                Content = ""
            };
            label.MouseLeftButtonDown += Source_MouseDown;
            Grid.SetColumn(label, curSourceNum);

            Grid.SetRow(label, 0);

            curSourceNum += 1;
            //Debug.WriteLine("source nums:" + curSourceNum);

            sourceContainer.Children.Add(label);
        }
        private void DeleteSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                if (label.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(label);
                }
            }
        }
        public void Addtrack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
            trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

            frameCanvas canvas = new frameCanvas
            {
                Width = currTrackWidth,
                Height = _tracksHeight,
                Background = Brushes.LightGray,
                Margin = new Thickness(1),
                AllowDrop = true,
            };
            Grid.SetColumn(canvas, 0);
            Grid.SetRow(canvas, 2 * curTracksNum+1);
            curTracksNum++;
            trackContainer.Children.Add(canvas);
        }
        private void SourceViewDrag_down(object sender, MouseButtonEventArgs e)
        {
            if (sourceScroll.IsMouseOver)
            {
                _MousePoint = e.GetPosition(sourceScroll);
                _horizontalOffset = sourceScroll.HorizontalOffset;
                sourceScroll.CaptureMouse();
            }
        }
        private void SourceViewDrag_Move(object sender, MouseEventArgs e)
        {
            if (sourceScroll.IsMouseCaptured)
            {
                var delta = _MousePoint.X - e.GetPosition(sourceScroll).X;
                sourceScroll.ScrollToHorizontalOffset(_horizontalOffset + delta);
            }
        }
        private void SourceViewDrag_Up(object sender, MouseButtonEventArgs e)
        {
            if (sourceScroll.IsMouseCaptured)
            {
                sourceScroll.ReleaseMouseCapture();
            }
        }
        private void Track_ScrollChange(object sender, ScrollChangedEventArgs e)
        {
            if (trackContainerScroll.HorizontalOffset + trackContainerScroll.ViewportWidth >= trackContainerScroll.ExtentWidth - 50)
            {
                ExpandLabelsWidth();
                currTrackWidth = trackContainerScroll.HorizontalOffset + trackContainerScroll.ViewportWidth;
            }
        }
        private void ExpandLabelsWidth()
        {
            foreach (UIElement element in trackContainer.Children)
            {
                if (element is frameCanvas label)
                {
                    label.Width += SourceWidth;
                    currTrackWidth = label.Width;
                }
            }
        }
        private void TrackViewDrag_down(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Drag down event trigger");
            if (trackContainerScroll.IsMouseOver)
            {
                _MousePoint = e.GetPosition(trackContainerScroll);
                _horizontalOffset = trackContainerScroll.HorizontalOffset;
                _verticalOffset = trackContainerScroll.VerticalOffset;
                trackContainerScroll.CaptureMouse();
            }
        }
        private void TrackViewDrag_Move(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("Drag move event trigger");
            if (trackContainerScroll.IsMouseCaptured)
            {
                var deltaX = _MousePoint.X - e.GetPosition(trackContainerScroll).X;
                trackContainerScroll.ScrollToHorizontalOffset(_horizontalOffset + deltaX);

                var deltaY = _MousePoint.Y - e.GetPosition(trackContainerScroll).Y;
                trackContainerScroll.ScrollToVerticalOffset(_verticalOffset + deltaY);
            }
        }
        private void TrackViewDrag_Up(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Drag up event trigger");
            if (trackContainerScroll.IsMouseCaptured)
            {
                trackContainerScroll.ReleaseMouseCapture();
            }
        }
    }
}
