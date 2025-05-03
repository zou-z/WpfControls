using ColorPicker.Component;
using ColorPicker.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ColorPicker
{
    [TemplatePart(Name = HuePickerName, Type = typeof(HuePicker))]
    [TemplatePart(Name = SaturationBrightnessPickerName, Type = typeof(SaturationBrightnessPicker))]
    [TemplatePart(Name = TransparencyPickerName, Type = typeof(TransparencyPicker))]
    [TemplatePart(Name = RedChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = GreenChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = BlueChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = AlphaChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = HueChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = SaturationChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = BrightnessChannelInputName, Type = typeof(TextBox))]
    [TemplatePart(Name = HexValueInputName, Type = typeof(TextBox))]
    public class ColorPicker : Control
    {
        public const string HuePickerName = "PART_HuePicker";
        public const string SaturationBrightnessPickerName = "PART_SaturationBrightnessPicker";
        public const string TransparencyPickerName = "PART_TransparencyPicker";
        public const string RedChannelInputName = "PART_RedChannelInput";
        public const string GreenChannelInputName = "PART_GreenChannelInput";
        public const string BlueChannelInputName = "PART_BlueChannelInput";
        public const string AlphaChannelInputName = "PART_AlphaChannelInput";
        public const string HueChannelInputName = "PART_HueChannelInput";
        public const string SaturationChannelInputName = "PART_SaturationChannelInput";
        public const string BrightnessChannelInputName = "PART_BrightnessChannelInput";
        public const string HexValueInputName = "PART_HexValueInput";

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.Gray));
        public static readonly DependencyProperty IsAlphaEnabledProperty =
            DependencyProperty.Register(nameof(IsAlphaEnabled), typeof(bool), typeof(ColorPicker), new PropertyMetadata(true, OnIsAlphaEnabled));

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public bool IsAlphaEnabled
        {
            get => (bool)GetValue(IsAlphaEnabledProperty);
            set => SetValue(IsAlphaEnabledProperty, value);
        }

        public override void OnApplyTemplate()
        {
            UpdateAlphaChannelValue();

            if (GetTemplateChild(HuePickerName) is not HuePicker huePicker ||
                GetTemplateChild(SaturationBrightnessPickerName) is not SaturationBrightnessPicker saturationBrightnessPicker ||
                GetTemplateChild(TransparencyPickerName) is not TransparencyPicker transparencyPicker ||
                GetTemplateChild(RedChannelInputName) is not TextBox redChannelInput ||
                GetTemplateChild(GreenChannelInputName) is not TextBox greenChannelInput ||
                GetTemplateChild(BlueChannelInputName) is not TextBox blueChannelInput ||
                GetTemplateChild(AlphaChannelInputName) is not TextBox alphaChannelInput ||
                GetTemplateChild(HueChannelInputName) is not TextBox hueChannelInput ||
                GetTemplateChild(SaturationChannelInputName) is not TextBox saturationChannelInput ||
                GetTemplateChild(BrightnessChannelInputName) is not TextBox brightnessChannelInput ||
                GetTemplateChild(HexValueInputName) is not TextBox hexValueInput)
            {
                return;
            }

            var hsvPickerCombination = new HsvPickerCombination(
                huePicker,
                saturationBrightnessPicker,
                transparencyPicker
            );
            var hsvInputCombination = new HsvInputCombination(
                hueChannelInput,
                saturationChannelInput,
                brightnessChannelInput
            );
            var rgbInputCombination = new RgbInputCombination(
                redChannelInput,
                greenChannelInput,
                blueChannelInput
            );
            var alphaInputCombination = new AlphaInputCombination(
                alphaChannelInput
            );
            hexInputCombination = new HexInputCombination(
                hexValueInput,
                IsAlphaEnabled
            );

            colorPropertyValueSync = new DependencyPropertyValueSync<Color>(
                this,
                ColorProperty,
                [
                    hsvPickerCombination,
                    hsvInputCombination,
                    rgbInputCombination,
                    alphaInputCombination,
                    hexInputCombination
                ],
                Color
            );
        }

        private void UpdateAlphaChannelValue()
        {
            if (!IsAlphaEnabled && Color.A != 255)
            {
                Color = Color.FromRgb(Color.R, Color.G, Color.B);
            }
        }

        private static void OnIsAlphaEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker self && e.NewValue is bool isAlphaEnabled)
            {
                if (self.hexInputCombination != null)
                {
                    self.hexInputCombination.IsAlphaEnabled = isAlphaEnabled;
                }
                self.UpdateAlphaChannelValue();
            }
        }

        private DependencyPropertyValueSync<Color>? colorPropertyValueSync = null;
        private HexInputCombination? hexInputCombination = null;

        private class HsvPickerCombination(
            HuePicker huePicker,
            SaturationBrightnessPicker saturationBrightnessPicker,
            TransparencyPicker transparencyPicker) : DependencyPropertyValueAggregator<Color>(
                [
                    new(huePicker, HuePicker.HueProperty),
                    new(saturationBrightnessPicker, SaturationBrightnessPicker.SaturationProperty),
                    new(saturationBrightnessPicker, SaturationBrightnessPicker.BrightnessProperty),
                    new(transparencyPicker, TransparencyPicker.AlphaProperty),
                ]
            )
        {
            protected override void OnUpdatePropertiesValue()
            {
                var aggregateValue = GetAggregateValue();
                var hsv = ColorSpaceUtil.RgbToHsv(aggregateValue);

                SetPropertyValue(huePicker, HuePicker.HueProperty, hsv.Hue);

                SetPropertyValue(saturationBrightnessPicker, SaturationBrightnessPicker.SaturationProperty, hsv.Saturation);
                SetPropertyValue(saturationBrightnessPicker, SaturationBrightnessPicker.BrightnessProperty, hsv.Value);

                SetPropertyValue(transparencyPicker, TransparencyPicker.AlphaProperty, aggregateValue.A);
            }

            protected override void OnUpdateAggregateValue(
                DependencyObject dependencyObject,
                DependencyProperty dependencyProperty,
                object? propertyValue,
                Action<Color> updateCallback)
            {
                var h = GetPropertyValue<double>(huePicker, HuePicker.HueProperty);
                var s = GetPropertyValue<double>(saturationBrightnessPicker, SaturationBrightnessPicker.SaturationProperty);
                var v = GetPropertyValue<double>(saturationBrightnessPicker, SaturationBrightnessPicker.BrightnessProperty);

                var color = ColorSpaceUtil.HsvToRgb(h, s, v);
                color.A = transparencyPicker.Alpha;

                updateCallback(color);
            }
        }

        private class HsvInputCombination(
            TextBox hueChannelInput,
            TextBox saturationChannelInput,
            TextBox brightnessChannelInput) : DependencyPropertyValueAggregator<Color>(
                [
                    new(hueChannelInput, TextBox.TextProperty),
                    new(saturationChannelInput, TextBox.TextProperty),
                    new(brightnessChannelInput, TextBox.TextProperty),
                ]
            )
        {
            protected override void OnUpdatePropertiesValue()
            {
                var hsv = ColorSpaceUtil.RgbToHsv(GetAggregateValue());

                SetPropertyValue(hueChannelInput, TextBox.TextProperty, ((int)hsv.Hue).ToString());
                SetPropertyValue(saturationChannelInput, TextBox.TextProperty, ((int)(hsv.Saturation * 100)).ToString());
                SetPropertyValue(brightnessChannelInput, TextBox.TextProperty, ((int)(hsv.Value * 100)).ToString());

                ClearAllErrorTipAdorner();
            }

            protected override void OnUpdateAggregateValue(
                DependencyObject dependencyObject,
                DependencyProperty dependencyProperty,
                object? propertyValue,
                Action<Color> updateCallback)
            {
                var text = propertyValue as string;
                if (!TryParseHsvChannelValue(text, 0, dependencyObject.Equals(hueChannelInput) ? 360 : 100, out var _))
                {
                    ErrorTipAdorner.ShowAdorner(dependencyObject);
                    return;
                }
                ErrorTipAdorner.ClearAdorner((UIElement)dependencyObject);

                if (TryParseHsvChannelValue(hueChannelInput.Text, 0, 360, out var hue) &&
                    TryParseHsvChannelValue(saturationChannelInput.Text, 0, 100, out var saturation) &&
                    TryParseHsvChannelValue(brightnessChannelInput.Text, 0, 100, out var brightness))
                {
                    var color = ColorSpaceUtil.HsvToRgb(hue, saturation / 100d, brightness / 100d);
                    updateCallback(Color.FromArgb(GetAggregateValue().A, color.R, color.G, color.B));
                }
            }

            private static bool TryParseHsvChannelValue(string? text, int minimum, int maximum, out int value)
            {
                if (text != null && int.TryParse(text, out value))
                {
                    if (minimum <= value && value <= maximum)
                    {
                        return true;
                    }
                }

                value = 0;
                return false;
            }

            private void ClearAllErrorTipAdorner()
            {
                ErrorTipAdorner.ClearAdorner(hueChannelInput);
                ErrorTipAdorner.ClearAdorner(saturationChannelInput);
                ErrorTipAdorner.ClearAdorner(brightnessChannelInput);
            }
        }

        private class RgbInputCombination(
            TextBox redChannelInput,
            TextBox greenChannelInput,
            TextBox blueChannelInput) : DependencyPropertyValueAggregator<Color>(
                [
                    new(redChannelInput, TextBox.TextProperty),
                    new(greenChannelInput, TextBox.TextProperty),
                    new(blueChannelInput, TextBox.TextProperty),
                ]
            )
        {
            protected override void OnUpdatePropertiesValue()
            {
                var aggregateValue = GetAggregateValue();

                SetPropertyValue(redChannelInput, TextBox.TextProperty, aggregateValue.R.ToString());
                SetPropertyValue(greenChannelInput, TextBox.TextProperty, aggregateValue.G.ToString());
                SetPropertyValue(blueChannelInput, TextBox.TextProperty, aggregateValue.B.ToString());

                ClearAllErrorTipAdorner();
            }

            protected override void OnUpdateAggregateValue(
                DependencyObject dependencyObject,
                DependencyProperty dependencyProperty,
                object? propertyValue,
                Action<Color> updateCallback)
            {
                var text = propertyValue as string;
                if (!TryParseColorChannelValue(text, out var _))
                {
                    ErrorTipAdorner.ShowAdorner(dependencyObject);
                    return;
                }
                ErrorTipAdorner.ClearAdorner((UIElement)dependencyObject);

                if (TryParseColorChannelValue(redChannelInput.Text, out var red) &&
                    TryParseColorChannelValue(greenChannelInput.Text, out var green) &&
                    TryParseColorChannelValue(blueChannelInput.Text, out var blue))
                {
                    updateCallback(Color.FromArgb(GetAggregateValue().A, red, green, blue));
                }
            }

            private static bool TryParseColorChannelValue(string? text, out byte value)
            {
                if (text != null && byte.TryParse(text, out value))
                {
                    return true;
                }

                value = 0;
                return false;
            }

            private void ClearAllErrorTipAdorner()
            {
                ErrorTipAdorner.ClearAdorner(redChannelInput);
                ErrorTipAdorner.ClearAdorner(greenChannelInput);
                ErrorTipAdorner.ClearAdorner(blueChannelInput);
            }
        }

        private class AlphaInputCombination(TextBox alphaChannelInput) :
            DependencyPropertyValueAggregator<Color>(
                [
                    new(alphaChannelInput, TextBox.TextProperty),
                ]
            )
        {
            protected override void OnUpdatePropertiesValue()
            {
                var aggregateValue = GetAggregateValue();
                SetPropertyValue(alphaChannelInput, TextBox.TextProperty, aggregateValue.A.ToString());
                ErrorTipAdorner.ClearAdorner(alphaChannelInput);
            }

            protected override void OnUpdateAggregateValue(
                DependencyObject dependencyObject,
                DependencyProperty dependencyProperty,
                object? propertyValue,
                Action<Color> updateCallback)
            {
                var text = propertyValue as string;
                if (!TryParseColorChannelValue(text, out var value))
                {
                    ErrorTipAdorner.ShowAdorner(dependencyObject);
                    return;
                }
                ErrorTipAdorner.ClearAdorner((UIElement)dependencyObject);

                var aggregateValue = GetAggregateValue();
                updateCallback(Color.FromArgb(value, aggregateValue.R, aggregateValue.G, aggregateValue.B));
            }

            private static bool TryParseColorChannelValue(string? text, out byte value)
            {
                if (text != null && byte.TryParse(text, out value))
                {
                    return true;
                }

                value = 0;
                return false;
            }
        }

        private class HexInputCombination(
            TextBox hexValueInput,
            bool isAlphaEnabled) : DependencyPropertyValueAggregator<Color>(
                [
                    new(hexValueInput, TextBox.TextProperty),
                ]
            )
        {
            public bool IsAlphaEnabled { get; set; } = isAlphaEnabled;

            protected override void OnUpdatePropertiesValue()
            {
                var aggregateValue = GetAggregateValue();
                var hexValue = IsAlphaEnabled ? $"{aggregateValue.A:X2}" : string.Empty;
                hexValue = $"{hexValue}{aggregateValue.R:X2}{aggregateValue.G:X2}{aggregateValue.B:X2}";
                SetPropertyValue(hexValueInput, TextBox.TextProperty, hexValue);

                ErrorTipAdorner.ClearAdorner(hexValueInput);
            }

            protected override void OnUpdateAggregateValue(
                DependencyObject dependencyObject,
                DependencyProperty dependencyProperty,
                object? propertyValue,
                Action<Color> updateCallback)
            {
                var text = propertyValue as string;
                if (!TryParseHexValue(text, out var color))
                {
                    ErrorTipAdorner.ShowAdorner(dependencyObject);
                    return;
                }
                ErrorTipAdorner.ClearAdorner((UIElement)dependencyObject);

                if (!IsAlphaEnabled && color.A != 255)
                {
                    text = $"FF{color.R:X2}{color.G:X2}{color.B:X2}";
                    SetPropertyValue(dependencyObject, dependencyProperty, text);
                }
                else if (text?.StartsWith('#') == true)
                {
                    SetPropertyValue(dependencyObject, dependencyProperty, text[1..]);
                }

                updateCallback(color);
            }

            private static bool TryParseHexValue(string? text, out Color color)
            {
                try
                {
                    text = text?.StartsWith('#') != true ? $"#{text}" : text;
                    if (ColorConverter.ConvertFromString(text) is Color parsedColor)
                    {
                        color = parsedColor;
                        return true;
                    }
                }
                catch { }
                return false;
            }
        }

        private class ErrorTipAdorner(UIElement adornedElement) : Adorner(adornedElement)
        {
            public static void ShowAdorner(DependencyObject dependencyObject)
            {
                var element = (UIElement)dependencyObject;
                var layer = AdornerLayer.GetAdornerLayer(element);
                if (layer.GetAdorners(element)?.Any(t => t is ErrorTipAdorner) != true)
                {
                    layer.Add(new ErrorTipAdorner(element));
                }
            }

            public static void ClearAdorner(UIElement element)
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adorner = layer.GetAdorners(element)?.FirstOrDefault(t => t is ErrorTipAdorner);
                if (adorner != null)
                {
                    layer.Remove(adorner);
                }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawRectangle(
                    null,
                    new Pen(Brushes.Red, 1),
                    new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height)
                );
            }
        }
    }
}
