using AudioStation.Model;
using AudioStation.ViewModels.Interface;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class NowPlayingViewModel : ViewModelBase, INowPlayingViewModel
    {
        string _artist;
        string _album;
        string _title;
        TimeSpan _currentTime;
        TimeSpan _duration;
        StreamSourceType _sourceType;
        string _source;

        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set { this.RaiseAndSetIfChanged(ref _currentTime, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public StreamSourceType SourceType
        {
            get { return _sourceType; }
            set { this.RaiseAndSetIfChanged(ref _sourceType, value); }
        }
        public string Source
        {
            get { return _source; }
            set { this.RaiseAndSetIfChanged(ref _source, value); }
        }

        public NowPlayingViewModel()
        {
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Title = string.Empty;
            this.Source = string.Empty;
        }
    }
}
