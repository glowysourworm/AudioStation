using System.Globalization;
using System.Windows.Data;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Multiplies a ratio (input) by the parameter
    /// </summary>
    public class RatioParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            var number = (double)parameter;
            var ratio = (double)value;

            return ratio * number;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
