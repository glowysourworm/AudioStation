using System.Globalization;
using System.Windows.Data;

using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

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

            if (values[0] is not LibraryImporterStagedFileCollection)
                return Binding.DoNothing;

            if (values[1] is not LibraryImporterStagedFileFilterType)
                return Binding.DoNothing;

            var collection = values[0] as LibraryImporterStagedFileCollection;
            var filterType = (LibraryImporterStagedFileFilterType)values[1];

            if (collection == null)
                return Binding.DoNothing;

            switch (filterType)
            {
                case LibraryImporterStagedFileFilterType.None:
                    return collection.Files;
                case LibraryImporterStagedFileFilterType.Valid:
                    return collection.ValidFiles;
                case LibraryImporterStagedFileFilterType.Invalid:
                    return collection.InvalidFiles;
                case LibraryImporterStagedFileFilterType.AcoustID:
                    return collection.AcoustIDFiles;
                case LibraryImporterStagedFileFilterType.MusicBrainzBasic:
                    return collection.MusicBrainzFiles;
                case LibraryImporterStagedFileFilterType.MusicBrainzSpecialTag:
                    return collection.MusicBrainzSpecialTagFiles;
                case LibraryImporterStagedFileFilterType.LibraryConflict:
                    return collection.LibraryConflictFiles;
                default:
                    throw new Exception("Unhandled staged file filter type");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
