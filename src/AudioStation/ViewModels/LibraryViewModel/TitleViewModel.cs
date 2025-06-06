using AudioStation.Core.Model;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModel.LibraryViewModel
{
    public class TitleViewModel : ViewModelBase
    {
        string _fileName;
        string _title;
        uint _track;
        TimeSpan _duration;

        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public uint Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }

        public TitleViewModel()
        {
            this.Track = 0;
            this.FileName = string.Empty;
            this.Title = string.Empty;
            this.Duration = TimeSpan.Zero;
        }
    }
}
