using AudioStation.Core.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

namespace AudioStation.ViewModels.MainViewModels.Interface
{
    public interface IPlaylistEntryViewModel
    {
        ArtistViewModel Artist { get; }
        AlbumViewModel Album { get; }
        LibraryEntryViewModel Track { get; }
        TimeSpan CurrentTime { get; }
        double CurrentTimeRatio { get; }
        bool IsPlaying { get; set; }

        void UpdateCurrentTime(TimeSpan currentTime);
    }
}
