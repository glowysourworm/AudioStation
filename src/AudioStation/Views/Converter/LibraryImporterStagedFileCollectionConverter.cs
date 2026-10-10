using System.Globalization;
using System.Windows.Data;

using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Multi-converter for taking the LibraryImporterStagedFileCollection + LibraryImporterStagedFileType and
    /// returning the proper collection
    /// </summary>
    public class LibraryImporterStagedFileCollectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return Binding.DoNothing;

            if (values.Length != 2)
                return Binding.DoNothing;

            if (values[0] is not LibraryImporterFileTreeViewModel)
                return Binding.DoNothing;

            if (values[1] is not LibraryImporterStagedFileFilterType)
                return Binding.DoNothing;

            var collection = values[0] as LibraryImporterFileTreeViewModel;
            var filterType = (LibraryImporterStagedFileFilterType)values[1];

            if (collection == null)
                return Binding.DoNothing;

            switch (filterType)
            {
                case LibraryImporterStagedFileFilterType.None:
                case LibraryImporterStagedFileFilterType.Valid:
                case LibraryImporterStagedFileFilterType.Invalid:
                case LibraryImporterStagedFileFilterType.AcoustID:
                case LibraryImporterStagedFileFilterType.MusicBrainzBasic:
                case LibraryImporterStagedFileFilterType.MusicBrainzSpecialTag:
                case LibraryImporterStagedFileFilterType.LibraryConflict:
                case LibraryImporterStagedFileFilterType.ImportReadyFiles:
                default:
                    //throw new Exception("Unhandled staged file filter type");
                    break;
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
