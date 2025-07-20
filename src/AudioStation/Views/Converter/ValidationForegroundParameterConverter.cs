using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    public class ValidationForegroundParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Red;

            else if ((bool)value)
                return ((Brush)parameter == null) ? Brushes.LawnGreen : (Brush)parameter;

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
