using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Applies a red brush when value is invalid
    /// </summary>
    public class ValidationBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            else if (!(bool)value)
                return Brushes.Red;

            return (Brush)parameter ?? Brushes.HotPink;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
