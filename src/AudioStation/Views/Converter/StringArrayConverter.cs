using System.Globalization;
using System.Windows.Data;

namespace AudioStation.Views.Converter
{
    public class StringArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var array = value as IEnumerable<string>;

            if (array != null)
            {
                return string.Join(',', array);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
