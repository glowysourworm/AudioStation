using AudioStation.Core.Model;
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
        int _bitrate;
        string _codec;
        double _currentTimeRatio;

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
            set { this.RaiseAndSetIfChanged(ref _currentTime, value); SetCurrentTimeRatio(); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); SetCurrentTimeRatio(); }
        }
        public double CurrentTimeRatio
        {
            get { return _currentTimeRatio; }
            set { this.RaiseAndSetIfChanged(ref _currentTimeRatio, value); }
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

        public int Bitrate
        {
            get { return _bitrate; }
            set { this.RaiseAndSetIfChanged(ref _bitrate, value); }
        }
        public string Codec
        {
            get { return _codec; }
            set { this.RaiseAndSetIfChanged(ref _codec, value); }
        }

        private void SetCurrentTimeRatio()
        {
            if (this.Duration.TotalMilliseconds <= 0)
                this.CurrentTimeRatio = 0;
            else
                this.CurrentTimeRatio = this.CurrentTime.TotalMilliseconds / this.Duration.TotalMilliseconds;
        }

        public NowPlayingViewModel()
        {
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Title = string.Empty;
            this.Source = string.Empty;
            this.Bitrate = 0;
            this.Codec = string.Empty;
        }
    }
}
