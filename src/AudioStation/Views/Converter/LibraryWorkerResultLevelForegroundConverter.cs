using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using AudioStation.Core.Component.LibraryLoaderComponent;

namespace AudioStation.Views.Converter
{
    public class LibraryWorkerResultLevelForegroundConverter : IValueConverter
    {
        private readonly Brush _unsetBrush;
        private readonly Brush _successBrush;
        private readonly Brush _serviceNoResultBrush;
        private readonly Brush _dataWarningBrush;
        private readonly Brush _dataErrorBrush;
        private readonly Brush _serviceFailureBrush;
        private readonly Brush _failureBrush;

        public LibraryWorkerResultLevelForegroundConverter()
        {
            _unsetBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_Unset");
            _successBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_Success");
            _serviceNoResultBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_ServiceNoResult");
            _dataWarningBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_DataWarning");
            _dataErrorBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_DataError");
            _serviceFailureBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_ServiceFailure");
            _failureBrush = (Brush)Application.Current.FindResource("LibraryWorkerResultLevel_Foreground_Failure");
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            if (value is not LibraryWorkerResultLevel)
                return Binding.DoNothing;

            var result = (LibraryWorkerResultLevel)value;

            switch (result)
            {
                case LibraryWorkerResultLevel.None:
                    return _unsetBrush;
                case LibraryWorkerResultLevel.Success:
                    return _successBrush;
                case LibraryWorkerResultLevel.ServiceNoResult:
                    return _serviceNoResultBrush;
                case LibraryWorkerResultLevel.DataWarning:
                    return _dataWarningBrush;
                case LibraryWorkerResultLevel.DataError:
                    return _dataErrorBrush;
                case LibraryWorkerResultLevel.ServiceFailure:
                    return _serviceFailureBrush;
                case LibraryWorkerResultLevel.Failure:
                    return _failureBrush;
                default:
                    throw new Exception("Unhandled LibraryWorkerResultLevel");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
