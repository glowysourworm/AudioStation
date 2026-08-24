using System.Globalization;
using System.Windows.Data;

using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;

namespace AudioStation.Views.Converter
{
    public class LibraryLoaderAcoustIDStateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var workItem = value as LibraryWorkItemViewModel;

            if (workItem == null)
                return Binding.DoNothing;

            if (workItem.InProgress)
                return "In Progress";

            else if (workItem.IsCompleted)
                return "Completed";

            else
                return "Queued";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
