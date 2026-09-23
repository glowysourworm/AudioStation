using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Payload.Input
{
    public class LibraryLoaderMusicBrainzBasicLoadViewModel : ViewModelBase
    {
        Guid? _musicBrainzTrackIDTag;
        Guid? _musicBrainzReleaseTrackIDTag;

        public Guid? MusicBrainzTrackIDTag
        {
            get { return _musicBrainzTrackIDTag; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzTrackIDTag, value); }
        }
        public Guid? MusicBrainzReleaseTrackIDTag
        {
            get { return _musicBrainzReleaseTrackIDTag; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzReleaseTrackIDTag, value); }
        }
    }
}
