namespace ColorPicker.Extension
{
    internal static class DoubleExtensions
    {
        public static double Clamp(this double value, double min, double max) => Math.Min(Math.Max(min, value), max);
    }
}
