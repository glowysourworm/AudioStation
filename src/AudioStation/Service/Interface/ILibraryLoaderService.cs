using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

using SimpleWpf.UI.ViewModel.FileTreeView;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService : IAudioStationService
    {
        /// <summary>
        /// Initializes Audio Station Library with entities from the database
        /// </summary>
        LibraryViewModel LoadLibrary(DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads NowPlaying data component with default for a track (the album is the playlist)
        /// </summary>
        NowPlayingViewModel GetNowPlaying(PlaylistEntryViewModel currentTrack);

        /// <summary>
        /// Loads NowPlaying data component with default for track (the album's tracks)
        /// </summary>
        NowPlayingPlaylistViewModel GetDefaultPlaylist(TrackViewModel track);

        /// <summary>
        /// Loads NowPlaying data component with default for an album (the album's tracks)
        /// </summary>
        NowPlayingPlaylistViewModel GetDefaultPlaylist(AlbumViewModel album);

        /// <summary>
        /// Loads NowPlaying data component with default for an artist (all artist's album's tracks)
        /// </summary>
        NowPlayingPlaylistViewModel GetDefaultPlaylist(ArtistViewModel artist);

        /// <summary>
        /// Loads a library entry page from the database
        /// </summary>
        PageResult<TrackViewModel> LoadEntryPage(PageRequest<Track, int> request);

        /// <summary>
        /// Initializes the library importer directory to recursion depth 0.
        /// </summary>
        FileTreeViewModel InitializeImporterTree(string directory,
                                                 LibraryImporterConfigurationViewModel importerOptions,
                                                 DialogProgressHandler progressHandler,
                                                 params string[] searchPatterns);

        /// <summary>
        /// Loads further directories of the importer tree
        /// </summary>
        void LoadImporterTreeNextDepth(FileTreeViewModel treeRoot,
                                        int currentDepth,
                                        DialogProgressHandler progressHandler,
                                        params string[] searchPatterns);
    }
}
