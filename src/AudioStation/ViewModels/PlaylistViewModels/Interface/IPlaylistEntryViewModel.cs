using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.ViewModels.PlaylistViewModels.Interface
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
