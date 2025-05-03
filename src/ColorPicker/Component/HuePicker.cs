using ColorPicker.Extension;
using ColorPicker.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorPicker.Component
{
    internal class HuePicker : PickerBase
    {
        public static readonly DependencyProperty HueProperty =
           DependencyProperty.Register(nameof(Hue), typeof(double), typeof(HuePicker), new PropertyMetadata(0d, OnHueChanged));
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(HuePicker), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        public HuePicker() : base(pickerRadius)
        {
            hueColorGradientStops =
            [
                new GradientStop(Color.FromRgb(255, 0, 0), 0 / 6d),
                new GradientStop(Color.FromRgb(255, 255, 0), 1 / 6d),
                new GradientStop(Color.FromRgb(0, 255, 0), 2 / 6d),
                new GradientStop(Color.FromRgb(0, 255, 255), 3 / 6d),
                new GradientStop(Color.FromRgb(0, 0, 255), 4 / 6d),
                new GradientStop(Color.FromRgb(255, 0, 255), 5 / 6d),
                new GradientStop(Color.FromRgb(255, 0, 0), 6 / 6d),
            ];
        }

        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnLoaded()
        {
            UpdateRender(UpdateRenderType.Position | UpdateRenderType.Color);
        }

        protected override void OnPositionChanged(Point position)
        {
            Hue = Orientation == Orientation.Horizontal
                ? position.X / ActualWidth * 360
                : position.Y / ActualHeight * 360;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var radius = (Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth) / 2;
            var rect = new Rect(0, 0, ActualWidth, ActualHeight);
            var hueColorBrush = new LinearGradientBrush(
                hueColorGradientStops,
                Orientation == Orientation.Horizontal ? 0 : 90
            );
            drawingContext.DrawRoundedRectangle(hueColorBrush, null, rect, radius, radius);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateRender(UpdateRenderType.Position);
        }

        private void UpdateRender(UpdateRenderType type)
        {
            var position = GetPosition();

            if ((type & UpdateRenderType.Position) == UpdateRenderType.Position)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    position.X = ActualWidth * Hue.Clamp(0, 360) / 360;
                    position.Y = ActualHeight / 2;
                    position.X = position.X.Clamp(pickerRadius, ActualWidth - pickerRadius);
                }
                else
                {
                    position.X = ActualWidth / 2;
                    position.Y = ActualHeight * Hue.Clamp(0, 360) / 360;
                    position.Y = position.Y.Clamp(pickerRadius, ActualHeight - pickerRadius);
                }
            }

            var color = (type & UpdateRenderType.Color) == UpdateRenderType.Color
                ? ColorSpaceUtil.HsvToRgb(Hue, 1, 1)
                : GetColor();

            UpdatePickerRender(position, color);
        }

        private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HuePicker self)
            {
                self.UpdateRender(UpdateRenderType.Position | UpdateRenderType.Color);
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HuePicker self)
            {
                self.InvalidateVisual();
                self.UpdateRender(UpdateRenderType.Position);
            }
        }

        private readonly GradientStopCollection hueColorGradientStops;
        private const double pickerRadius = 7;

        private enum UpdateRenderType
        {
            Position = 1 << 0,
            Color = 1 << 1,
        }
    }
}
