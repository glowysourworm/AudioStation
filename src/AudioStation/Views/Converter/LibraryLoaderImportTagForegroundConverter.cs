using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using AudioStation.ViewModels.LibraryLoaderViewModels;

namespace AudioStation.Views.Converter
{
    public class LibraryLoaderImportTagForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return Binding.DoNothing;

            if (values.Length != 3)
                return Binding.DoNothing;

            var isTagDirty = (bool)values[0];
            var isImportValid = (bool)values[1];
            var isInError = (bool)values[2];

            if (isImportValid)
                return Brushes.LawnGreen;

            // Fields have been added to the tag in memory (also from online services)
            else if (isTagDirty && isImportValid)
                return Brushes.DodgerBlue;

            else if (isTagDirty && !isImportValid)
                return Brushes.Violet;

            else if (!isImportValid || isInError)
                return Brushes.Red;

            else
                return Brushes.DimGray;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
