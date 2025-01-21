using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI_v1;

public class KnobControl : Control
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(KnobControl),
        new PropertyMetadata(0.0, OnValueChanged));
    private MainWindow _mainwindow;
    public KnobControl(MainWindow mainWindow)
    {
        _mainwindow  = mainWindow;
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set
        {
            double clampedValue = Math.Max(0, Math.Min(1, value));
            SetValue(ValueProperty, clampedValue);
        }
    }

    public static readonly DependencyProperty IsOnProperty =
        DependencyProperty.Register("IsOn", typeof(bool), typeof(KnobControl),
        new PropertyMetadata(false, OnIsOnChanged));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    private double _startAngle;
    private double _startValue;
    private Point _center;

    static KnobControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KnobControl),
            new FrameworkPropertyMetadata(typeof(KnobControl)));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        Brush knobColor = IsOn ? Brushes.Green : Brushes.LightGray;

        _center = new Point(ActualWidth / 2, ActualHeight / 2);
        double radius = Math.Min(ActualWidth, ActualHeight) / 2 - 10;

        dc.DrawEllipse(knobColor, new Pen(Brushes.DarkGray, 2), _center, radius, radius);

        if (Value > 0)
        {
            double filledAngle = Value * 360;
            double radiansStart = -Math.PI / 2;

            double radiansEnd = radiansStart + filledAngle * Math.PI / 180;
            Point arcEndPoint = new Point(
                _center.X + radius * Math.Cos(radiansEnd),
                _center.Y + radius * Math.Sin(radiansEnd)
            );

            PathFigure pathFigure = new PathFigure
            {
                StartPoint = _center,
                IsClosed = true
            };

            pathFigure.Segments.Add(new LineSegment(
                new Point(_center.X, _center.Y - radius),
                true
            ));

            ArcSegment arcSegment = new ArcSegment
            {
                Point = arcEndPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = filledAngle > 180
            };
            pathFigure.Segments.Add(arcSegment);

            pathFigure.Segments.Add(new LineSegment(_center, true));

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(112, 112, 255)), null, pathGeometry);
        }

        double pointerAngle = Value * 360 * Math.PI / 180 - Math.PI / 2;
        Point pointer = new Point(
            _center.X + radius * Math.Cos(pointerAngle),
            _center.Y + radius * Math.Sin(pointerAngle)
        );
        dc.DrawLine(new Pen(Brushes.Black, 3), _center, pointer);
    }


    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            IsOn = !IsOn;
            if (IsOn)
            {
                _startAngle = CalculateAngle(e.GetPosition(this));
                _startValue = Value;
                Mouse.Capture(this);
            }
            else
            {
                Mouse.Capture(null);
            }

            InvalidateVisual();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsOn && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(this);
            double currentAngle = CalculateAngle(currentPosition);
            double angleDelta = currentAngle - _startAngle;

            if (angleDelta < -180) angleDelta += 360;
            else if (angleDelta > 180) angleDelta -= 360;

            double newValue = Math.Max(0, Math.Min(1, _startValue + angleDelta / 360));

            if ((Value == 0 && newValue < Value) || (Value == 1 && newValue > Value))
            {
                return;
            }
            Value = newValue;
        }
    }
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        double delta = e.Delta > 0 ? 0.03 : -0.03;
        double newValue = Math.Max(0, Math.Min(1, Value + delta));

        if ((Value == 0 && newValue < Value) || (Value == 1 && newValue > Value))
        {
            return;
        }

        Value = newValue;
        
        var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
        Debug.WriteLine("parent name : " + parent.Name);
        string sampleName = _mainwindow.TrackNumberToSampleName[parent.Name.Substring(10)];

        if (parent != null && parent.Name.StartsWith("Controller")) 
        {
            if (_mainwindow.patternDictionary.TryGetValue(sampleName, out var trackData))
            {
                trackData.Volume = Value;
            }
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        Mouse.Capture(null);
    }

    private double CalculateAngle(Point position)
    {
        double dx = position.X - _center.X;
        double dy = position.Y - _center.Y;
        double angle = Math.Atan2(dy, dx) * 180 / Math.PI;

        return angle < 0 ? angle + 360 : angle;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        KnobControl knob = d as KnobControl;

        knob.InvalidateVisual();
    }



    private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        KnobControl knob = d as KnobControl;
        knob.InvalidateVisual();
    }
}
