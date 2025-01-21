using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using Microsoft.Win32;
using System.IO;
using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Data.Common;
using System.Xml.Linq;
using System.Reflection.Emit;
using Label = System.Windows.Controls.Label;

namespace UI_v1
{
    public partial class MainWindow : Window
    {
        int octave = 0;
        int curSourceNum = 0;
        int curTracksNum = 3;
        int trackNumber = 0;
        int stepNumber = 0;

        double currTrackWidth;
        double stepWidth;

        List<string> samplePaths = new List<string>();
        string sourcepath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "source\\");
        string iconpath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "icon\\");


        public static double SourceWidth = 0;
        public static double SourceHeight = 0;

        public static double TrackControllerWidth = 0;
        public static double TrackControllerHeight = 0;

        public static int SectionOfLatestNote = 8;

        private bool _isInitialized = false;
        private bool _isSelectLabel = false;

        private Point _MousePoint;

        private int _curSelectedTrackNum;
        private int _curMeasureNum;

        private double _horizontalOffset;
        private double _verticalOffset;
        static double _tracksHeight = 0;

        private Dictionary<(int row, int column), UIElement> elementMap = new Dictionary<(int, int), UIElement>();

        private DispatcherTimer _scrollUpdateTimer;

        private sampleLabel _draggedShadowLabel;
        private sampleLabel _copyLabel = new sampleLabel();
        private sampleLabel curSelectedLabel;

        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += MainWindow_SizeChanged;
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitialized = true;
            SetDefaultSample();
            SetPianoRow();
            SetSources();
            SetTracks();
            SetCircleBtn();
        }
        private void SetDefaultSample()
        {
            drumMachine = new DrumMachine();

            if (Directory.Exists(sourcepath))
            {
                var extensions = new[] { "*.wav", "*.mp3", "*.flac" };
                var wavFiles = extensions.SelectMany(ext => Directory.GetFiles(sourcepath, ext));

                foreach (var file in wavFiles)
                {
                    string fileName = Path.GetFileName(file);
                    Debug.WriteLine(fileName);
                    samplePaths.Add(fileName);
                }
                drumMachine.LoadSamples(samplePaths);
            }
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
                    Tag = i - 13
                };// 2 4 7 9 11 black

                if (i % 12 == 2 || i % 12 == 4 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11)
                {
                    label.Background = Brushes.Black;
                }
                else
                {
                    label.Background = Brushes.White;
                }
                Grid.SetColumn(label, i - 1);
                Grid.SetRow(label, 0);

                label.PreviewMouseDown += PianoRowClick;

                keyContainer.Children.Add(label);
            }

            Label up = new Label
            {
                Name = "octaveUp",
                Content = "▲",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.LightGray),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 20
            };
            Label down = new Label
            {
                Name = "octaveDown",
                Content = "▼",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Gray),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 20
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
        }
        public void OctaveDown(object sender, MouseButtonEventArgs e)
        {
            octave--;
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

            for (int i = 0; i < 24; i++)
            {
                sourceContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });
            }
            Random r = new Random();

            if (Directory.Exists(sourcepath))
            {
                Debug.WriteLine("sourcepath: " + sourcepath);
                var extensions = new[] { "*.wav", "*.mp3", "*.flac" };
                var wavFiles = extensions.SelectMany(ext => Directory.GetFiles(sourcepath, ext));

                int i = 1;
                foreach (var file in wavFiles)
                {
                    string fileName = Path.GetFileName(file).Substring(0, Path.GetFileName(file).Length - 4);

                    Brush brush = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                                  (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
                    string name = fileName;
                    string iconName = "";
                    iconName = "kick.png";
                    sampleLabel label = new sampleLabel
                    {
                        Name = name,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Background = brush,
                        Margin = new Thickness(1),
                        Content = new Image
                        {
                            Source = new BitmapImage(new Uri(iconpath + iconName, UriKind.Absolute)),
                            Stretch = Stretch.Fill,
                            Width = SourceWidth,
                            Height = SourceHeight,
                        }
                    };
                    label.PreviewMouseLeftButtonDown += Source_MouseDown;
                    label.PreviewMouseDoubleClick += Source_Preview;

                    Border border = new Border
                    {
                        Background = brush,
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        CornerRadius = new CornerRadius(label.CornerRadius)
                    };
                    border.Child = label;

                    border.Width = SourceWidth;
                    border.Height = SourceHeight;

                    border.Clip = new RectangleGeometry
                    {
                        Rect = new System.Windows.Rect(0, 0, SourceWidth- SourceWidth*0.05, SourceHeight- SourceHeight*0.05),
                        RadiusX = 10,
                        RadiusY = 10
                    };
                    Grid.SetColumn(border, i - 1);
                    Grid.SetRow(border, 0);
                    i++;

                    curSourceNum += 1;
                    sourceContainer.Children.Add(border);
                }

                // control scroll bar
                sourceScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sourceScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                sourceScroll.PreviewMouseDown += SourceViewDrag_down;
                sourceScroll.PreviewMouseMove += SourceViewDrag_Move;
                sourceScroll.PreviewMouseUp += SourceViewDrag_Up;
            }
        }
        private void PianoRowClick(object sender, MouseButtonEventArgs e)
        {
            Label label = null;

            if (_isSelectLabel && sender is Label selectedLabel)
            {
                label = selectedLabel;

                if (octave >= 0)
                {
                    curSelectedLabel.tone = Int32.Parse(label.Tag.ToString()) + octave * 36;
                }
                else
                {
                    curSelectedLabel.tone = (Int32.Parse(label.Tag.ToString()) - 35) - (octave + 1) * 36;
                }

                var track = VisualTreeHelper.GetParent(curSelectedLabel) as frameCanvas;
                patternDictionary[curSelectedLabel.Name].Patterns[curSelectedLabel.currMeasure].tone = curSelectedLabel.tone;


                var temp = label.Background;
                label.Background = Brushes.Gray;

                Task.Delay(200).ContinueWith(_ =>
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        label.Background = temp == Brushes.Black ? Brushes.Black : Brushes.White;
                    });
                });
            }
            else if (sender is Label otherLabel)
            {
                label = otherLabel;
            }
            if (label != null)
            {
                var temp = label.Background;
                label.Background = Brushes.Gray;

                Task.Delay(200).ContinueWith(_ =>
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        if (temp == Brushes.White)
                        {
                            label.Background = Brushes.White;
                        }
                        else
                        {
                            label.Background = temp;
                        }
                    });
                });
            }
        }
        private sampleLabel CopyLabel(sampleLabel sourceLabel)
        {
            if (sourceLabel == null) return null;

            sampleLabel newLabel = new sampleLabel
            {
                Name = sourceLabel.Name,
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

                Bass = sourceLabel.Bass,
                Mid = sourceLabel.Mid,
                Treble = sourceLabel.Treble,
            };

            return newLabel;
        }
        private void Source_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _copyLabel = CopyLabel(sender as sampleLabel);
            CreateShadowLabel(_copyLabel, e);
            DragDrop.DoDragDrop(_copyLabel, _copyLabel.Content, DragDropEffects.Copy);
        }
        private void Source_Preview(object sender, MouseButtonEventArgs e)
        {
            if (sender is sampleLabel sample)
            {
                string previewPath;
                if (!File.Exists(sourcepath + sample.Name + ".wav"))
                    previewPath = sourcepath + sample.Name + ".mp3";
                else
                    previewPath = sourcepath + sample.Name + ".wav";

                using (var previewAudio = new AudioFileReader(previewPath))
                using (var previewOutputs = new WaveOutEvent())
                {
                    previewOutputs.Init(previewAudio);
                    previewOutputs.Play();

                    var maxPreviewTime = TimeSpan.FromSeconds(3);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    while (previewOutputs.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);

                        if (stopwatch.Elapsed >= maxPreviewTime)
                        {
                            previewOutputs.Stop();
                        }
                    }
                }
            }
        }
        private void CreateShadowLabel(sampleLabel sourceLabel, MouseButtonEventArgs e)
        {
            Debug.WriteLine("CreateShadowLabel");

            _draggedShadowLabel = new sampleLabel
            {
                FontSize = sourceLabel.FontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Gray),
                RenderTransform = new TranslateTransform(),
                Height = SourceHeight,
                Width = SourceWidth,
            };
            var originalImage = (Image)sourceLabel.Content;
            var newImage = new Image
            {
                Source = originalImage.Source,
                Stretch = originalImage.Stretch,
                Width = originalImage.Width,
                Height = originalImage.Height,
            };

            _draggedShadowLabel.Content = newImage;
            if (e.Source is frameCanvas track)
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

            transform.X = position.X - 12 * (SourceWidth);
            transform.Y = position.Y - (SourceHeight / 2);
            Debug.WriteLine("transform: " + transform.X);
        }
        private void Target_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is frameCanvas track)
            {
                sampleLabel newLabel = new sampleLabel
                {
                    Name = _copyLabel.Name,
                    Content = _copyLabel.Content,
                    Height = SourceHeight,
                    Width = SourceWidth,
                    Background = _copyLabel.Background
                };
                var originalImage = (Image)_copyLabel.Content;
                var newImage = new Image
                {
                    Source = originalImage.Source,
                    Stretch = originalImage.Stretch,
                    Width = originalImage.Width,
                    Height = originalImage.Height,
                };
                newLabel.Content = newImage;

                Border border = new Border
                {
                    Background = _copyLabel.Background,
                    CornerRadius = new CornerRadius(_copyLabel.CornerRadius)
                };
                //border.Child = newLabel;

                border.Width = SourceWidth;
                border.Height = SourceHeight;

                border.Clip = new RectangleGeometry
                {
                    Rect = new System.Windows.Rect(0, 0, SourceWidth, SourceHeight),
                    RadiusX = 10,
                    RadiusY = 10
                };

                newLabel.MouseRightButtonDown += DeleteSource_MouseDown;
                newLabel.MouseLeftButtonDown += SelectSource_MouseDown;

                var position = e.GetPosition(track);
                double x = Math.Floor(position.X / SourceWidth);
                double y = Math.Floor(position.Y / SourceHeight);

                double posX = Convert.ToInt16(x) * SourceWidth;
                double posY = Convert.ToInt16(y) * SourceHeight;
                Canvas.SetLeft(newLabel, posX);
                Canvas.SetTop(newLabel, posY);
                track.Children.Add(newLabel);

                int trackNum = int.Parse(track.Name.Substring(5));
                elementMap[((int)x, trackNum)] = newLabel;
                Debug.WriteLine("映射進去 " + (int)x + ", " + trackNum);

                if (SectionOfLatestNote < (int)y)
                {
                    Debug.WriteLine("Before changing SectionOfLatestNote: " + SectionOfLatestNote.ToString());
                    Debug.WriteLine("Before changing pattern list length: " + patternDictionary.Count.ToString());

                    SectionOfLatestNote = (int)x;

                    Debug.WriteLine("After changing SectionOfLatestNote: " + SectionOfLatestNote.ToString());
                    Debug.WriteLine("After changing pattern list length: " + patternDictionary.Count.ToString());
                }
                //int currDictLen = patternDictionary[_copyLabel.Name].Patterns.Count();
                Debug.WriteLine("Target_Drop");

                Debug.WriteLine("This source is " + _copyLabel.Name);
                Debug.WriteLine("This measure is " + x + " and it is in " + track.Name);
                //SectionOfLatestNote
                if (!patternDictionary.ContainsKey(_copyLabel.Name))
                {
                    if(!TrackNumberToSampleName.ContainsKey(track.Name.Substring(5)))
                    {
                        List<PatternEQ> tempList = Enumerable.Range(0, SectionOfLatestNote + 1)
                                         .Select(_ => new PatternEQ { IsActive = false }).ToList();
                        SampleData tempdata = new SampleData();
                        tempdata.Patterns = tempList;
                        patternDictionary.Add(_copyLabel.Name, tempdata);

                        int requiredLength = (int)x;

                        for (int i = 0; i < requiredLength; i++)
                        {
                            patternDictionary[_copyLabel.Name].Patterns.Add(new PatternEQ { IsActive = false });
                        }

                        TrackNumberToSampleName.Add(track.Name.Substring(5), _copyLabel.Name);//'已經加入含有相同索引鍵的項目
                        patternDictionary[_copyLabel.Name].Patterns[(int)x].IsActive = true;
                        muteTrackList.Add(_copyLabel.Name, true);//false代表我不要靜音
                    }
                    else
                    {
                        MessageBox.Show("同個Pattern不可以放入不同Sample!");
                        DeleteChildByIndex(track, (int)x, trackNum);
                        Debug.WriteLine("已刪除 ("+ (int)x + ", " + trackNum + ")");
                    }

                }
                else
                {
                    int currDictLen = patternDictionary[_copyLabel.Name].Patterns.Count();
                    if (currDictLen < (int)x + 1)
                    {
                        int requiredLength = (int)x - currDictLen + 1;
                        Debug.WriteLine("currDictLen = " + currDictLen);
                        Debug.WriteLine("SectionOfLatestNote = " + SectionOfLatestNote);
                        Debug.WriteLine("requiredLength = " + requiredLength);

                        for (int i = 0; i < requiredLength; i++)
                        {
                            patternDictionary[_copyLabel.Name].Patterns.Add(new PatternEQ { IsActive = false });
                        }
                    }
                    patternDictionary[_copyLabel.Name].Patterns[(int)x].IsActive = true;//bug System.ArgumentOutOfRangeException: '索引超出範圍。必須為非負數且小於集合的大小。
                }
                SectionOfLatestNote = SectionOfLatestNote > (int)x ? SectionOfLatestNote : (int)x;
                foreach(var pattern in patternDictionary)
                {
                    int currDictLen = pattern.Value.Patterns.Count();
                    if (currDictLen < SectionOfLatestNote)
                    {
                        int requiredLength = SectionOfLatestNote - currDictLen + 1;
                        for (int i = 0; i < requiredLength; i++)
                        {
                            pattern.Value.Patterns.Add(new PatternEQ { IsActive = false });
                        }
                    }
                }
            }
        }
        private void DeleteChildByIndex(Canvas canvas, int row, int column)
        {
            if (elementMap.TryGetValue((row, column), out UIElement elementToRemove))
            {
                canvas.Children.Remove(elementToRemove);
                elementMap.Remove((row, column));
            }
            else
            {
                MessageBox.Show($"No element found at row {row}, column {column}.");
            }
        }


        private void SwitchMouse_down(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label trackSwitch)
            {
                Debug.WriteLine("trigger switch!!!");
                int numLength = trackSwitch.Name.Length - 6;
                string switchNumber = trackSwitch.Name.Substring(6, numLength);

                muteTrackList[TrackNumberToSampleName[switchNumber]] = !muteTrackList[TrackNumberToSampleName[switchNumber]];
                Debug.WriteLine("被我靜音了嗎");
                foreach (var data in muteTrackList)
                {
                    Debug.WriteLine("muteTrackList key: " + data.Value);
                }


                if (!muteTrackList[TrackNumberToSampleName[switchNumber]])
                {
                    trackSwitch.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else
                {
                    trackSwitch.Background = new SolidColorBrush(Color.FromRgb(49, 225, 85));
                }
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

            _tracksHeight = source.ActualHeight;

            //measure
            MeasureHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });

            measureCanvas measureCanvas = new measureCanvas
            {
                Name = "measure",
                Width = SourceWidth,
                Height = MeasureHeader.ActualHeight,
                Background = Brushes.LightGray,
                Margin = new Thickness(1)
            };

            measureCanvas.Loaded += (s, e) =>
            {
                measureCanvas.Width = stepWidth;
            };

            Grid.SetColumn(measureCanvas, 0);
            Grid.SetRow(measureCanvas, 0);

            MeasureHeader.Children.Add(measureCanvas);
            for (int i = 0; i < 7; i++)
            {
                //switch
                trackSwitch.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
                trackSwitch.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

                Label switchLabel = new Label
                {
                    Name = "Switch" + trackNumber.ToString(),
                    Width = SourceWidth,
                    Height = _tracksHeight,
                    Background = new SolidColorBrush(Color.FromRgb(112, 255, 112)),
                };
                switchLabel.PreviewMouseDown += SwitchMouse_down;
                Grid.SetRow(switchLabel, 2 * i + 1);
                trackSwitch.Children.Add(switchLabel);

                //trackContainer
                trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
                trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

                frameCanvas trackCanvas = new frameCanvas
                {
                    Name = "track" + trackNumber.ToString(),
                    Width = SourceWidth,
                    Height = _tracksHeight,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(1),
                    AllowDrop = true
                };
                trackCanvas.Loaded += (s, e) =>
                {
                    trackCanvas.Width = SourceWidth;
                };

                Grid.SetColumn(trackCanvas, 0);
                Grid.SetRow(trackCanvas, 2 * i + 1);

                trackContainer.Children.Add(trackCanvas);

                //trackController
                double actualWidth = trackController.ActualWidth;
                double actualHeight = source.ActualHeight;
                TrackControllerWidth = (actualWidth / 18);
                TrackControllerHeight = actualHeight;

                trackController.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
                trackController.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

                trackControllerCanvas controller = new trackControllerCanvas(this)
                {
                    Name = "Controller" + trackNumber.ToString(),
                    Height = _tracksHeight,
                    Background = Brushes.White,
                };

                Grid.SetRow(controller, 2 * i + 1);
                trackController.Children.Add(controller);

                trackNumber += 1;

            }
            /* ----- measureHeader Setting -----*/
            MeasureHeaderScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            MeasureHeaderScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            /* ----- TrackSwitch Setting -----*/
            trackSwitchScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //trackSwitchScroll.PreviewMouseDown += SwitchMouse_down;


            /* ----- TrackContainer Setting -----*/
            //controll scroll bar
            trackContainerScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            trackContainerScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            trackContainerScroll.ScrollChanged += Track_ScrollChange;

            trackContainerScroll.PreviewMouseDown += TrackMouse_down;
            trackContainerScroll.PreviewMouseMove += TrackMouse_Move;
            trackContainerScroll.PreviewMouseUp += TrackMouse_Up;

            trackContainerScroll.DragOver += Target_DragOver;
            trackContainerScroll.Drop += Target_Drop;

            /* ----- TrackController Setting -----*/
            trackControllerScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            trackControllerScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }
        public void Addsource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ConfirmWindow confirmWindow = new ConfirmWindow();
            confirmWindow.UserChoice += (isChoiceA) =>
            {
                if (isChoiceA)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.flac";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        string sourceFilePath = openFileDialog.FileName;
                        string fileName = Path.GetFileName(sourceFilePath);
                        string name = Path.GetFileNameWithoutExtension(sourceFilePath);

                        Debug.WriteLine("sourceFilePath123 = " + sourceFilePath);
                        Debug.WriteLine("fileName123 = " + fileName);
                        Debug.WriteLine("name123 = " + name);
                        string cc = Directory.GetCurrentDirectory();
                        string pp = Directory.GetParent(cc).Parent.FullName;
                        Debug.WriteLine("pp123 = " + pp);
                        Debug.WriteLine("cc123 = " + cc);


                        if (!samplePaths.Contains(fileName))
                        {
                            string currentDirectory = Directory.GetCurrentDirectory();
                            string parentDirectory = Directory.GetParent(currentDirectory).Parent.FullName;
                            string filePath = Path.Combine(parentDirectory, "source", fileName);
                            File.Copy(sourceFilePath, filePath);
                            Debug.WriteLine("sourceFilePath123 = " + sourceFilePath);
                            sourceContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });
                            Random r = new Random();
                            Brush brush = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                                          (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
                            sampleLabel label = new sampleLabel
                            {
                                Name = name,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                Background = brush,
                                Margin = new Thickness(1),
                                Content = ""
                            };
                            label.MouseLeftButtonDown += Source_MouseDown;
                            label.PreviewMouseDoubleClick += Source_Preview;
                            Border border = new Border
                            {
                                Background = brush,
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(label.CornerRadius)
                            };
                            border.Child = label;

                            border.Width = SourceWidth;
                            border.Height = SourceHeight;

                            border.Clip = new RectangleGeometry
                            {
                                Rect = new System.Windows.Rect(0, 0, SourceWidth - SourceWidth * 0.05, SourceHeight - SourceHeight * 0.05),
                                RadiusX = 10,
                                RadiusY = 10
                            };
                            Grid.SetColumn(border, curSourceNum);
                            Grid.SetRow(border, 0);

                            curSourceNum += 1;
                            sourceContainer.Children.Add(border);

                            Debug.WriteLine("載入成功: " + fileName);

                            samplePaths.Add(fileName);
                            drumMachine.LoadSamples(samplePaths);
                            MessageBox.Show("為這個聲音選一張圖片作為icon吧");
                            OpenFileDialog iconDialog = new OpenFileDialog();
                            iconDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                            iconDialog.InitialDirectory = iconpath;
                            if (iconDialog.ShowDialog() == true)
                            {
                                string iconPath = iconDialog.FileName;

                                label.Content = new Image
                                {
                                    Source = new BitmapImage(new Uri(iconPath)),
                                    Stretch = Stretch.Uniform
                                };
                            }
                        }
                        else
                        {
                            MessageBox.Show("已經選過了!");
                        }
                    }
                }
                else
                {
                    ConfirmWindow2 inputWindow = new ConfirmWindow2();
                    if (inputWindow.ShowDialog() == true)
                    {
                        string string1 = inputWindow.String1;
                        string string2 = inputWindow.String2;
                        float number = inputWindow.Number;
                        string apiKey = "sk_f126364b713b29b77320fafce48940e5e043de7a7cfc75ba";

                        string pythonExePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName
                                                            , "ExternalTools\\dist\\11labtest.exe");
                        string pythonScriptPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName
                                                            , "ExternalTools\\11labtest.python");
                        Debug.WriteLine("pythonExePath: " + pythonExePath);

                        string1 = string1.ToString();
                        string arguments = $"\"{string1}\" \"{string2}\" {number}";

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = pythonExePath,
                            Arguments = $" --text \"{string1}\" --output_file_name \"{string2}\" --sample_time {number}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };
                        try
                        {
                            using (Process process = Process.Start(startInfo))
                            {
                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();
                                process.WaitForExit(10000);

                                Debug.WriteLine("Python output:");
                                Debug.WriteLine(output);

                                if (!string.IsNullOrEmpty(error))
                                {
                                    Debug.WriteLine("Python error:");
                                    Debug.WriteLine(error);
                                }
                            }

                            sourceContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SourceWidth, GridUnitType.Pixel) });

                            Random r = new Random();
                            Brush brush = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                                          (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
                            sampleLabel label = new sampleLabel
                            {
                                Name = string2,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                Background = brush,
                                Margin = new Thickness(1),
                                Content = ""
                            };
                            label.MouseLeftButtonDown += Source_MouseDown;
                            label.PreviewMouseDoubleClick += Source_Preview;

                            Border border = new Border
                            {
                                Background = brush,
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                CornerRadius = new CornerRadius(label.CornerRadius)
                            };
                            border.Child = label;

                            border.Width = SourceWidth;
                            border.Height = SourceHeight;

                            border.Clip = new RectangleGeometry
                            {
                                Rect = new System.Windows.Rect(0, 0, SourceWidth - SourceWidth * 0.05, SourceHeight - SourceHeight * 0.05),
                                RadiusX = 10,
                                RadiusY = 10
                            };
                            Grid.SetColumn(border, curSourceNum);
                            Grid.SetRow(border, 0);

                            curSourceNum += 1;
                            sourceContainer.Children.Add(border);
                            Debug.WriteLine("載入成功1: " + string2);

                            string2 = string2 + ".mp3";
                            Debug.WriteLine("名字是啥: " + string2);
                            samplePaths.Add(string2);
                            drumMachine.LoadSamples(samplePaths);
                            MessageBox.Show("為這個聲音選一張圖片作為icon吧");
                            OpenFileDialog iconDialog = new OpenFileDialog();
                            iconDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                            iconDialog.InitialDirectory = iconpath;
                            if (iconDialog.ShowDialog() == true)
                            {
                                string iconPath = iconDialog.FileName;

                                label.Content = new Image
                                {
                                    Source = new BitmapImage(new Uri(iconPath)),
                                    Stretch = Stretch.Uniform
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"oh no, something went wrong: {ex.Message}");
                        }
                    }
                }
            };
            confirmWindow.ShowDialog();
        }
        static void p_outputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendText(e.Data + Environment.NewLine);
            }
        }
        public delegate void AppendTextCallBack(string text);
        public static void AppendText(string text)
        {
            Console.WriteLine(text);
        }
        private void DeleteSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is sampleLabel label)
            {
                if (label.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(label);
                }
            }
        }
        private void SelectSource_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is sampleLabel label)
            {
                Debug.WriteLine("It is a note!");

                var track = VisualTreeHelper.GetParent(label) as frameCanvas;
                var position = e.GetPosition(track);
                int x = (int)Math.Floor(position.X / SourceWidth);
                int y = Int32.Parse(track.Name.Substring(5));
                _curSelectedTrackNum = y;
                _curMeasureNum = x;

                _isSelectLabel = true;
                curSelectedLabel = label;
                curSelectedLabel.tone = label.tone;
                BassSlider.Value = label.Bass;
                MidSlider.Value = label.Mid;
                TrebleSlider.Value = label.Treble;
                curSelectedLabel.currTrack = y;
                curSelectedLabel.currMeasure = x;
                Debug.WriteLine("curSelectedLabel: " + curSelectedLabel.Name);
                Debug.WriteLine("_curSelectedTrackNum: " + _curSelectedTrackNum);

                Debug.WriteLine("Bass: " + curSelectedLabel.Bass);
                Debug.WriteLine("Mid: " + curSelectedLabel.Mid);
                Debug.WriteLine("Treble: " + curSelectedLabel.Treble);
                Debug.WriteLine("tone: " + curSelectedLabel.tone);
            }
        }
        public void Addtrack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Trigger add track");
            trackNumber += 1;
            Debug.WriteLine("trackNumber: " + trackNumber);

            //trackSwitch
            trackSwitch.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
            trackSwitch.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

            Label switchLabel = new Label
            {
                Name = "Switch" + trackNumber.ToString(),
                Width = SourceWidth,
                Height = _tracksHeight,
                Background = new SolidColorBrush(Color.FromRgb(49, 225, 85)),
            };
            switchLabel.PreviewMouseDown += SwitchMouse_down;
            Grid.SetRow(switchLabel, 2 * (trackNumber - 1) + 1);
            trackSwitch.Children.Add(switchLabel);

            //trackContainer
            trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
            trackContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

            frameCanvas canvas = new frameCanvas
            {
                Name = "track" + trackNumber.ToString(),
                Width = currTrackWidth,
                Height = _tracksHeight,
                Background = Brushes.LightGray,
                Margin = new Thickness(1),
                AllowDrop = true,
            };

            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.VerticalAlignment = VerticalAlignment.Top;

            Grid.SetColumn(canvas, 0);
            Grid.SetRow(canvas, 2 * (trackNumber - 1) + 1);
            curTracksNum++;
            trackContainer.Children.Add(canvas);

            //trackController
            trackController.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight * 0.1, GridUnitType.Pixel) });
            trackController.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_tracksHeight, GridUnitType.Pixel) });

            trackControllerCanvas controller = new trackControllerCanvas(this)
            {
                Name = "Controller" + trackNumber.ToString(),
                Height = _tracksHeight,
                Background = Brushes.White,
            };

            Grid.SetRow(controller, 2 * (trackNumber - 1) + 1);
            trackController.Children.Add(controller);
        }
        private void SourceViewDrag_down(object sender, MouseButtonEventArgs e)
        {
            if (sourceScroll.IsMouseOver && e.MiddleButton == MouseButtonState.Pressed)
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
            if (_scrollUpdateTimer == null)
            {
                _scrollUpdateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(50)
                };
                _scrollUpdateTimer.Tick += (s, args) =>
                {
                    _scrollUpdateTimer.Stop();
                    MeasureHeaderScroll.ScrollToHorizontalOffset(trackContainerScroll.HorizontalOffset);
                };
            }
            _scrollUpdateTimer.Stop();
            _scrollUpdateTimer.Start();

            double visibleRightEdge = trackContainerScroll.HorizontalOffset + trackContainerScroll.ViewportWidth;

            if (visibleRightEdge >= trackContainerScroll.ExtentWidth - 50)
            {
                currTrackWidth = visibleRightEdge;
                stepWidth = visibleRightEdge;
                ExpandLabelsWidth();
            }
        }
        private void TrackMouse_down(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("TrackMouse_down triggered");
            if (trackContainerScroll.IsMouseOver)
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    _MousePoint = e.GetPosition(trackContainerScroll);
                    _horizontalOffset = trackContainerScroll.HorizontalOffset;
                    _verticalOffset = trackContainerScroll.VerticalOffset;
                    trackContainerScroll.CaptureMouse();
                }
                else if (e.RightButton == MouseButtonState.Pressed && e.Source is Image image)
                {
                    Debug.WriteLine("we are in second if");

                    var sample = FindParentSampleLabel(image);
                    if (sample != null)
                    {
                        var track = VisualTreeHelper.GetParent(sample) as frameCanvas;
                        if (track != null)
                        {
                            var position = e.GetPosition(track);
                            int x = (int)Math.Floor(position.X / SourceWidth);
                            int y = Int32.Parse(track.Name.Substring(5));
                            Debug.WriteLine($"We delete ({x}, {y})");

                            patternDictionary[sample.Name].Patterns[x].IsActive = false;
                        }
                    }
                }
            }
        }
        private sampleLabel FindParentSampleLabel(DependencyObject current)
        {
            while (current != null)
            {
                if (current is sampleLabel sample)
                {
                    return sample;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
        private void TrackMouse_Move(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                if (trackContainerScroll.IsMouseCaptured)
                {
                    var deltaX = _MousePoint.X - e.GetPosition(trackContainerScroll).X;
                    trackContainerScroll.ScrollToHorizontalOffset(_horizontalOffset + deltaX);
                    MeasureHeaderScroll.ScrollToHorizontalOffset(_horizontalOffset + deltaX);

                    var deltaY = _MousePoint.Y - e.GetPosition(trackContainerScroll).Y;
                    trackContainerScroll.ScrollToVerticalOffset(_verticalOffset + deltaY);
                    trackSwitchScroll.ScrollToVerticalOffset(_verticalOffset + deltaY);
                    trackControllerScroll.ScrollToVerticalOffset(_verticalOffset + deltaY);
                }
            }
        }
        private void TrackMouse_Up(object sender, MouseButtonEventArgs e)
        {
            if (trackContainerScroll.IsMouseCaptured)
            {
                if (e.MiddleButton == MouseButtonState.Released)
                {
                    trackContainerScroll.ReleaseMouseCapture();
                }
            }
        }
        private void ExpandLabelsWidth()
        {
            double totalWidth = 0;

            foreach (UIElement element in trackContainer.Children)
            {
                if (element is frameCanvas label)
                {
                    label.Width += SourceWidth;
                    currTrackWidth = label.Width;
                }
            }

            foreach (UIElement element in MeasureHeader.Children)
            {
                if (element is measureCanvas measure)
                {
                    measure.Width += SourceWidth;
                    totalWidth += measure.Width;
                }
            }
            MeasureHeader.Width = totalWidth;
            stepWidth = totalWidth;
        }
        private void BassSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSelectLabel)
            {
                curSelectedLabel.Bass = BassSlider.Value;
                changeDic_EQ("Bass", (float)BassSlider.Value);
            }
        }
        private void MidSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSelectLabel)
            {
                curSelectedLabel.Mid = MidSlider.Value;
                changeDic_EQ("Mid", (float)MidSlider.Value);
            }
        }
        private void TrebleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSelectLabel)
            {
                curSelectedLabel.Treble = TrebleSlider.Value;
                changeDic_EQ("Treble", (float)TrebleSlider.Value);
            }
        }
        private void changeDic_EQ(string part, float value)
        {
            SampleData sampleData = patternDictionary[curSelectedLabel.Name];
            if (part == "Bass")
            {
                sampleData.Patterns[_curMeasureNum].Bass = value;
            }
            else if (part == "Mid")
            {
                sampleData.Patterns[_curMeasureNum].Mid = value;
            }
            else if (part == "Treble")
            {
                sampleData.Patterns[_curMeasureNum].Treble = value;
            }
        }
        private void LoadMusic(object sender, RoutedEventArgs e)
        {

        }
        public void Stop(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.drumMachine.StopMusic();
        }
        private void Bpm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Bpm.Text.EndsWith(" BPM"))
            {
                string currentText = Bpm.Text.TrimEnd();
                if (int.TryParse(currentText, out int result))
                {
                    Bpm.Text = $"{result} BPM";
                    Bpm.SelectionStart = Bpm.Text.Length - 4;
                }
            }//哈哈根本沒用
        }
        private void StartTime_Change(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("time change");
            if (StartTime.Text.EndsWith(" 小節"))
            {
                Debug.WriteLine("enter time change if");
                string currentText = StartTime.Text.TrimEnd();
                Debug.WriteLine("currentText=" + currentText.Substring(0, currentText.Length - 3));

                if (int.TryParse(currentText.Substring(0, currentText.Length - 3), out int result))
                {
                    Debug.WriteLine("StartTime_R=" + result);
                    StartTime.Text = $"{result} 小節";
                    StartTime.SelectionStart = StartTime.Text.Length - 3;
                }
                else
                {
                    Debug.WriteLine("int.TryParse(currentText, out int result)" + int.TryParse(currentText, out int r));
                }
            }
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            //if (ChildControl != null)
            //{
            //    e.Handled = true; // 阻止 ScrollViewer 自己處理滾輪
            //    var routedEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            //    {
            //        RoutedEvent = UIElement.MouseWheelEvent
            //    };
            //    ChildControl.RaiseEvent(routedEvent); // 發送事件給子元素
            //}
        }

    }
}
