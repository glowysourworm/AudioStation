using System.Globalization;
using System.Windows.Data;

namespace AudioStation.Views.Converter
{
    public class BoolToParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var boolean = (bool)value;

            if (boolean)
                return parameter;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
