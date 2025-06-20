using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace AudioStation.Views.Converter
{
    public class AlternationIndexBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var index = (int)value;

            if (index == 0)
                return Brushes.Transparent;

            else
                return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
