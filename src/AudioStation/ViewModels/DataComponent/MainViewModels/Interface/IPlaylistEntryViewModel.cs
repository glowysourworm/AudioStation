using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.ViewModels.DataComponent.MainViewModels.Interface
{
    public interface IPlaylistEntryViewModel
    {
        ArtistViewModel Artist { get; }
        AlbumViewModel Album { get; }
        TrackViewModel Track { get; }
        TimeSpan CurrentTime { get; }
        double CurrentTimeRatio { get; }
        bool IsPlaying { get; set; }

        void UpdateCurrentTime(TimeSpan currentTime);
    }
}
