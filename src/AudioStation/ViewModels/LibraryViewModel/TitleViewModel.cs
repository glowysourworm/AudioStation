using AudioStation.Model;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModel.LibraryViewModel
{
    public class TitleViewModel : ViewModelBase
    {
        string _fileName;
        LibraryEntry _entry;
        string _name;
        uint _track;
        TimeSpan _duration;
        bool _nowPlaying;

        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public LibraryEntry Entry
        {
            get { return _entry; }
            set { this.RaiseAndSetIfChanged(ref _entry, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
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
        public bool NowPlaying
        {
            get { return _nowPlaying; }
            set { this.RaiseAndSetIfChanged(ref _nowPlaying, value); }
        }

        public TitleViewModel()
        {
            this.Track = 0;
            this.Entry = null;
            this.FileName = string.Empty;
            this.Name = string.Empty;
            this.Duration = TimeSpan.Zero;
            this.NowPlaying = false;
        }
    }
}
