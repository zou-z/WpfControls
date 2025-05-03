using ColorPicker.Extension;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorPicker.Component
{
    internal class TransparencyPicker : PickerBase
    {
        public static readonly DependencyProperty AlphaProperty =
           DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(TransparencyPicker), new PropertyMetadata(byte.MinValue, OnAlphaChanged));
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(TransparencyPicker), new PropertyMetadata(Colors.Black, OnColorChanged));
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(TransparencyPicker), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        public TransparencyPicker() : base(pickerRadius)
        {
            transparencyBackgroundBrush = new DrawingBrush()
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 8, 8),
                ViewportUnits = BrushMappingMode.Absolute,
                Drawing = new DrawingGroup()
                {
                    Children =
                    [
                        new GeometryDrawing(Brushes.LightGray, null, new RectangleGeometry(new Rect(4, 0, 4, 4))),
                        new GeometryDrawing(Brushes.LightGray, null, new RectangleGeometry(new Rect(0, 4, 4, 4))),
                    ]
                }
            };
        }

        public byte Alpha
        {
            get => (byte)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnLoaded()
        {
            UpdateRender();
        }

        protected override void OnPositionChanged(Point position)
        {
            Alpha = (byte)(Orientation == Orientation.Horizontal
              ? position.X / ActualWidth * 255
              : position.Y / ActualHeight * 255);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var radius = (Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth) / 2;
            var rect = new Rect(0, 0, ActualWidth, ActualHeight);

            var transparentColor = Color;
            transparentColor.A = 0;
            var opaqueColor = Color;
            opaqueColor.A = 255;

            var colorBrush = new LinearGradientBrush(
                transparentColor,
                opaqueColor,
                Orientation == Orientation.Horizontal ? 0 : 90
            );
            drawingContext.DrawRoundedRectangle(transparencyBackgroundBrush, null, rect, radius, radius);
            drawingContext.DrawRoundedRectangle(colorBrush, null, rect, radius, radius);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateRender();
        }

        private void UpdateRender()
        {
            var position = GetPosition();

            if (Orientation == Orientation.Horizontal)
            {
                position.X = ActualWidth * ((double)Alpha).Clamp(0, 255) / 255;
                position.Y = ActualHeight / 2;
                position.X = position.X.Clamp(pickerRadius, ActualWidth - pickerRadius);
            }
            else
            {
                position.X = ActualWidth / 2;
                position.Y = ActualHeight * ((double)Alpha).Clamp(0, 255) / 255;
                position.Y = position.Y.Clamp(pickerRadius, ActualHeight - pickerRadius);
            }

            UpdatePickerRender(position, Colors.Transparent);
        }

        private static void OnAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TransparencyPicker self)
            {
                self.UpdateRender();
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TransparencyPicker self)
            {
                self.InvalidateVisual();
                self.UpdateRender();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TransparencyPicker self)
            {
                self.InvalidateVisual();
                self.UpdateRender();
            }
        }

        private readonly DrawingBrush transparencyBackgroundBrush;
        private const double pickerRadius = 7;
    }
}
