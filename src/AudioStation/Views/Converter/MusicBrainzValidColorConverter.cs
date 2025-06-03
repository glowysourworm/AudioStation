using System.Globalization;
using System.Windows.Data;

using AudioStation.ViewModels;

namespace AudioStation.Views.Converter
{
    public class MusicBrainzValidColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return MainViewModel.DefaultMusicBrainzBackground;

            return (bool)value ? MainViewModel.ValidMusicBrainzBackground : MainViewModel.InvalidMusicBrainzBackground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
