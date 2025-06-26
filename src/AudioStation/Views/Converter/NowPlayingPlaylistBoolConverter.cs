using System.Globalization;
using System.Windows.Data;

using AudioStation.Event;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.Converter
{
    public class NowPlayingPlaylistBoolConverter : IValueConverter
    {
        private readonly IIocEventAggregator _eventAggregator;

        private IEnumerable<IPlaylistEntryViewModel> _playlistEntries;

        public NowPlayingPlaylistBoolConverter()
        {
            _eventAggregator = IocContainer.Get<IIocEventAggregator>();

            _eventAggregator.GetEvent<LoadPlaylistEvent>().Subscribe(eventData =>
            {
                _playlistEntries = eventData.NowPlayingData.Entries;
            });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || _playlistEntries == null)
                return Binding.DoNothing;

            var playlistEntry = value as IPlaylistEntryViewModel;

            if (playlistEntry != null)
                return _playlistEntries.Any(x => x.Track.Id == playlistEntry.Track.Id);

            var track = value as LibraryEntryViewModel;
            if (track != null)
                return _playlistEntries.Any(x => x.Track.Id == track.Id);

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
