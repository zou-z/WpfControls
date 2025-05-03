using ColorPicker.Extension;
using ColorPicker.Util;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker.Component
{
    internal class SaturationBrightnessPicker : PickerBase
    {
        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register(nameof(Hue), typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(0d, OnPropertyChanged));
        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(0d, OnPropertyChanged));
        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register(nameof(Brightness), typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(0d, OnPropertyChanged));

        public SaturationBrightnessPicker() : base(7)
        {
            opacityMaskBrush = new LinearGradientBrush(Colors.White, Colors.Transparent, 90);
            hueColorBrush = new LinearGradientBrush(Colors.White, Colors.White, 0);
        }

        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public double Brightness
        {
            get => (double)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        protected override void OnLoaded()
        {
            UpdateRender(UpdateRenderType.PositionX | UpdateRenderType.PositionY | UpdateRenderType.Color);
        }

        protected override void OnPositionChanged(Point position)
        {
            Saturation = position.X / ActualWidth;
            Brightness = 1 - position.Y / ActualHeight;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = new Rect(0, 0, ActualWidth, ActualHeight);
            hueColorBrush.GradientStops[1].Color = ColorSpaceUtil.HsvToRgb(Hue, 1, 1);

            drawingContext.DrawRectangle(Brushes.Black, null, rect);

            drawingContext.PushOpacityMask(opacityMaskBrush);
            drawingContext.DrawRectangle(hueColorBrush, null, rect);
            drawingContext.Pop();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateRender(sizeInfo.WidthChanged && sizeInfo.HeightChanged
                ? UpdateRenderType.PositionX | UpdateRenderType.PositionY
                : (sizeInfo.WidthChanged ? UpdateRenderType.PositionX : UpdateRenderType.PositionY)
            );
        }

        private void UpdateRender(UpdateRenderType type)
        {
            var position = GetPosition();

            position.X = (type & UpdateRenderType.PositionX) == UpdateRenderType.PositionX
                ? Saturation.Clamp(0, 1) * ActualWidth
                : position.X;

            position.Y = (type & UpdateRenderType.PositionY) == UpdateRenderType.PositionY
                ? (1 - Brightness.Clamp(0, 1)) * ActualHeight
                : position.Y;

            var color = (type & UpdateRenderType.Color) == UpdateRenderType.Color
                ? ColorSpaceUtil.HsvToRgb(Hue, Saturation, Brightness)
                : GetColor();

            UpdatePickerRender(position, color);

            if (type == UpdateRenderType.Color)
            {
                InvalidateVisual();
            }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SaturationBrightnessPicker self)
            {
                self.UpdateRender(e.Property.Name switch
                {
                    nameof(Hue) => UpdateRenderType.Color,
                    nameof(Saturation) => UpdateRenderType.PositionX | UpdateRenderType.Color,
                    nameof(Brightness) => UpdateRenderType.PositionY | UpdateRenderType.Color,
                    _ => UpdateRenderType.PositionX | UpdateRenderType.PositionY | UpdateRenderType.Color,
                });
            }
        }

        private readonly LinearGradientBrush opacityMaskBrush;
        private readonly LinearGradientBrush hueColorBrush;

        private enum UpdateRenderType
        {
            PositionX = 1 << 0,
            PositionY = 1 << 1,
            Color = 1 << 2,
        }
    }
}
