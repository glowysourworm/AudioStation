using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.LibraryViewModels.LibraryTree;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

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
        /// Importer Tree:  The importer (source) tree is an import tree based on the import directory that has
        ///                 the library entity grouping format used to build the tree for the initial viewing of
        ///                 the import load.
        ///                 
        ///                 The groupings are based on the (custom) library format for that folder; and the user
        ///                 may use any of the fields in the format specification to group the eventual tracks.
        /// </summary>
        void LoadImporterTree(FileTreeViewModel treeRoot,
                              LibraryImporterConfigurationViewModel workflowConfiguration,
                              DialogProgressHandler progressHandler,
                              params string[] searchPatterns);
    }
}
