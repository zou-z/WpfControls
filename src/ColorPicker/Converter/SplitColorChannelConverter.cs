using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorPicker.Converter
{
    internal class SplitColorChannelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return parameter switch
                {
                    nameof(Color.A) => color.A,
                    nameof(Color.R) => color.R,
                    nameof(Color.G) => color.G,
                    nameof(Color.B) => color.B,
                    _ => throw new NotImplementedException()
                };
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
