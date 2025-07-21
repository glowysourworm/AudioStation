using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Gives foreground colors for an import step for an icon. The value is a boolean. The parameter
    /// is a Brush, or null.
    /// </summary>
    public class ImportStepForegroundParameterConverter : IValueConverter
    {
        public static Brush ImportStepForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ImportStepForeground;

            else if ((bool)value)
                return ((Brush)parameter == null) ? Brushes.DodgerBlue : (Brush)parameter;

            return ImportStepForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
