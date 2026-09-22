using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    public class LibraryLoaderImportTagForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return Binding.DoNothing;

            if (values.Length != 2)
                return Binding.DoNothing;

            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            var isValid = (bool)values[0];
            var isModified = (bool)values[1];

            if (isValid)
            {
                if (isModified)
                    return Brushes.LawnGreen;

                else
                    return Brushes.LightGray;
            }
            else
            {
                return Brushes.Red;
            }
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
