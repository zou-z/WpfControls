using ColorPicker.Extension;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ColorPicker.Component
{
    internal class PickerBase : FrameworkElement
    {
        public PickerBase(double pickerRadius)
        {
            adorner = new PickerAdorner(this, pickerRadius);
            Loaded += PickerBase_Loaded;
            IsVisibleChanged += PickerBase_IsVisibleChanged;
        }

        protected virtual void OnLoaded() { }

        protected virtual void OnPositionChanged(Point position) { }

        protected void UpdatePickerRender(Point position, Color color)
        {
            adorner.Position = position;
            adorner.Color = color;
            adorner.InvalidateVisual();
        }

        protected Point GetPosition() => adorner.Position;

        protected Color GetColor() => adorner.Color;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Mouse.Capture(this))
            {
                UpdatePosition(e.GetPosition(this));
                isPressed = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            isPressed = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isPressed)
            {
                UpdatePosition(e.GetPosition(this));
            }
        }

        private void PickerBase_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoaded();
        }

        private void PickerBase_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                AdornerLayer.GetAdornerLayer(this).Add(adorner);
            }
            else if (e.NewValue is false)
            {
                AdornerLayer.GetAdornerLayer(this).Remove(adorner);
            }
        }

        private void UpdatePosition(Point point)
        {
            point.X = point.X.Clamp(0, ActualWidth);
            point.Y = point.Y.Clamp(0, ActualHeight);
            OnPositionChanged(point);
        }

        private readonly PickerAdorner adorner;
        private bool isPressed = false;

        private class PickerAdorner : Adorner
        {
            public static readonly DependencyProperty PositionProperty =
                DependencyProperty.Register(nameof(Position), typeof(Point), typeof(PickerAdorner), new PropertyMetadata(new Point()));
            public static readonly DependencyProperty ColorProperty =
                DependencyProperty.Register(nameof(Color), typeof(Color), typeof(PickerAdorner), new PropertyMetadata(Colors.Black));

            public PickerAdorner(UIElement adornedElement, double pickerRadius) : base(adornedElement)
            {
                this.pickerRadius = pickerRadius;
                IsHitTestVisible = false;
                SnapsToDevicePixels = true;
                Effect = new DropShadowEffect()
                {
                    Color = Colors.Black,
                    ShadowDepth = 0,
                    BlurRadius = 4,
                };
            }

            public Point Position
            {
                get => (Point)GetValue(PositionProperty);
                set => SetValue(PositionProperty, value);
            }

            public Color Color
            {
                get => (Color)GetValue(ColorProperty);
                set => SetValue(ColorProperty, value);
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawEllipse(
                    new SolidColorBrush(Color),
                    new Pen(Brushes.White, 2),
                    Position,
                    pickerRadius,
                    pickerRadius
                );
            }

            private readonly double pickerRadius;
        }
    }
}
