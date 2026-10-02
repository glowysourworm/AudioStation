using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.ViewModels.DataComponent.MainViewModels.Interface
{
    public interface IPlaylistEntryViewModel
    {
        /// <summary>
        /// Pointer to the Libary's (unique) artist
        /// </summary>
        ArtistViewModel Artist { get; }

        /// <summary>
        /// Pointer to the Library's (unique) album
        /// </summary>
        AlbumViewModel Album { get; }

        /// <summary>
        /// Pointer to the Library's (unique) track
        /// </summary>
        TrackViewModel Track { get; }

        TimeSpan CurrentTime { get; }
        double CurrentTimeRatio { get; }
        bool IsPlaying { get; set; }

        void UpdateCurrentTime(TimeSpan currentTime);
    }
}
