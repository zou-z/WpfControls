using ColorPicker.Extension;
using System.Windows.Media;

namespace ColorPicker.Util
{
    internal record HsvColor(double Hue, double Saturation, double Value);

    internal static class ColorSpaceUtil
    {
        public static Color HsvToRgb(double h, double s, double v)
        {
            h = (h % 360).Clamp(0, 360);
            s = s.Clamp(0, 1);
            v = v.Clamp(0, 1);

            var c = v * s;
            var x = c * (1 - Math.Abs((h / 60 % 2) - 1));
            var m = v - c;

            double r = 0, g = 0, b = 0;

            if (0 <= h && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (60 <= h && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (120 <= h && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (180 <= h && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (240 <= h && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (300 <= h && h < 360)
            {
                r = c; g = 0; b = x;
            }

            byte R = (byte)((r + m) * 255);
            byte G = (byte)((g + m) * 255);
            byte B = (byte)((b + m) * 255);

            return Color.FromRgb(R, G, B);
        }

        public static HsvColor RgbToHsv(Color color)
        {
            var red = ((double)color.R / 255).Clamp(0, 1);
            var green = ((double)color.G / 255).Clamp(0, 1);
            var blue = ((double)color.B / 255).Clamp(0, 1);

            var max = Math.Max(red, Math.Max(green, blue));
            var min = Math.Min(red, Math.Min(green, blue));
            var delta = max - min;

            var hue = 0d;
            if (delta != 0)
            {
                if (max == red)
                {
                    hue = 60 * ((green - blue) / delta);
                    if (hue < 0) hue += 360;
                }
                else if (max == green)
                {
                    hue = 60 * (2 + (blue - red) / delta);
                }
                else if (max == blue)
                {
                    hue = 60 * (4 + (red - green) / delta);
                }
            }

            var saturation = max == 0 ? 0 : delta / max;
            var value = max;

            return new HsvColor(hue, saturation, value);
        }
    }
}
