using AudioStation.Core.Component;
using AudioStation.Core.Model;

using SimpleWpf.Extensions;

namespace AudioStation.Event.DialogEvents
{
    public class DialogSmallAudioPlayerViewModel : ViewModelBase
    {
        string _fileName;
        StreamSourceType _sourceType;
        string _artist;
        string _album;
        string _track;

        TimeSpan _currentTime;
        TimeSpan _duration;
        double _currentTimeRatio;
        PlayStopPause _playState;

        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public StreamSourceType SourceType
        {
            get { return _sourceType; }
            set { this.RaiseAndSetIfChanged(ref _sourceType, value); }
        }
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
        public string Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set { this.RaiseAndSetIfChanged(ref _currentTime, value); UpdateCurrentTimeRatio(); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); UpdateCurrentTimeRatio(); }
        }
        public double CurrentTimeRatio
        {
            get { return _currentTimeRatio; }
            set { this.RaiseAndSetIfChanged(ref _currentTimeRatio, value); }
        }
        public PlayStopPause PlayState
        {
            get { return _playState; }
            set { this.RaiseAndSetIfChanged(ref _playState, value); }
        }

        private void UpdateCurrentTimeRatio()
        {
            if (this.Duration.TotalMilliseconds <= 0)
                this.CurrentTimeRatio = 0;
            else
                this.CurrentTimeRatio = this.CurrentTime.TotalMilliseconds / this.Duration.TotalMilliseconds;
        }

        public DialogSmallAudioPlayerViewModel()
        {
        }
    }
}
