using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Service
{
    [IocExport(typeof(ILibraryLoaderService))]
    public class LibraryLoaderService : ILibraryLoaderService
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IAudioStationMapper _audioStationMapper;

        // Initialize (prior to loading)
        IAudioStationComponentController _audioStationComponentController;

        [IocImportingConstructor]
        public LibraryLoaderService(IAudioStationDbClient audioStationDbClient, IAudioStationMapper audioStationMapper)
        {
            _audioStationDbClient = audioStationDbClient;
            _audioStationMapper = audioStationMapper;
        }

        public void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            _audioStationComponentController = audioStationController.ComponentController;
        }
        public LibraryViewModel LoadLibrary(DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            // Load Searchable Data (except for the library entries)
            try
            {
                var libraryViewModel = _audioStationComponentController.GetDataComponent<LibraryViewModel>();

                // Clear out any old data
                libraryViewModel.Dispose();

                // Load Order:  General -> Specific
                LoadGenres(ref libraryViewModel, progressHandler);
                LoadArtists(ref libraryViewModel, progressHandler);
                LoadTracks(ref libraryViewModel, progressHandler);

                return libraryViewModel;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error Loading Audio Station Entities:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public NowPlayingViewModel GetNowPlaying(PlaylistEntryViewModel currentTrack)
        {
            var libraryViewModel = _audioStationComponentController.GetDataComponent<LibraryViewModel>();
            var nowPlaying = _audioStationComponentController.GetDataComponent<NowPlayingViewModel>();

            nowPlaying.Dispose();

            nowPlaying.ArtistBio = "Artist Bio";
            nowPlaying.ArtistSummary = "Artist Summary";

            return nowPlaying;
        }

        public NowPlayingPlaylistViewModel GetDefaultPlaylist(TrackViewModel track)
        {
            var libraryViewModel = _audioStationComponentController.GetDataComponent<LibraryViewModel>();
            var nowPlaying = _audioStationComponentController.GetDataComponent<NowPlayingPlaylistViewModel>();

            nowPlaying.Dispose();

            var tracks = libraryViewModel.Tracks.Where(x => x.Album == track.Album);
            var album = libraryViewModel.Albums.First(x => x.Album == track.Album);
            var artist = libraryViewModel.Artists.First(x => x.Artist == track.Artist);

            nowPlaying.Playlist = new PlaylistViewModel();
            nowPlaying.Playlist.Entries.AddRange(tracks.Select(x => new PlaylistEntryViewModel(artist, album, track)));
            nowPlaying.Playlist.Name = "New Playlist";
            nowPlaying.SetPlaying(nowPlaying.Playlist.Entries.First(x => x.Track.Id == track.Id));

            return nowPlaying;
        }
        public NowPlayingPlaylistViewModel GetDefaultPlaylist(AlbumViewModel album)
        {
            return GetDefaultPlaylist(album.Media.First().Tracks.First());
        }
        public NowPlayingPlaylistViewModel GetDefaultPlaylist(ArtistViewModel artist)
        {
            var libraryViewModel = _audioStationComponentController.GetDataComponent<LibraryViewModel>();
            var nowPlaying = _audioStationComponentController.GetDataComponent<NowPlayingPlaylistViewModel>();

            nowPlaying.Dispose();

            nowPlaying.Playlist = new PlaylistViewModel();

            foreach (var album in artist.Albums)
            {
                var tracks = libraryViewModel.Tracks.Where(x => x.Album == album.Album);

                nowPlaying.Playlist
                          .Entries
                          .AddRange(tracks.Select(x => new PlaylistEntryViewModel(artist, album, x)));
            }

            nowPlaying.Playlist.Name = "New Playlist";
            nowPlaying.SetPlaying(nowPlaying.Playlist.Entries.First());

            return nowPlaying;
        }

        public PageResult<TrackViewModel> LoadEntryPage(PageRequest<Track, int> request)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            var result = new PageResult<TrackViewModel>();

            //// Database:  Load the file (entry) entities
            //var entryPage = _audioStationDbClient.GetPage(request);

            //result.PageNumber = request.PageNumber;
            //result.PageSize = request.PageSize;
            //result.TotalRecordCountFiltered = entryPage.TotalRecordCountFiltered;
            //result.TotalRecordCount = entryPage.TotalRecordCount;
            //result.Results = entryPage.Results.Select(_libraryMapperService.MapTrack).ToList();

            return result;
        }

        public FileTreeViewModel InitializeImporterTree(
                                                 string directory,
                                                 LibraryImporterConfigurationViewModel importerOptions,
                                                 DialogProgressHandler progressHandler,
                                                 params string[] searchPatterns)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            try
            {
                // Load first depth of the tree (TODO: Fix showing only the root, instead of starting with the child nodes)
                var result = DirectoryTreeLoader.Load<FileTreeViewModel>(directory, -1, (baseDirectory, currentPath, fileCount, parent) =>
                {
                    return new FileTreeViewModel(baseDirectory, currentPath, fileCount, parent);

                }, progressHandler, searchPatterns);

                // NOTE:  This is needed for multi-select inside of the SimpleTreeView
                result.SetTreeNumbering();

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public void LoadImporterTreeNextDepth(
                        FileTreeViewModel treeRoot,
                        int currentDepth,
                        DialogProgressHandler progressHandler,
                        params string[] searchPatterns)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            try
            {
                DirectoryTreeLoader.LoadToDepth(treeRoot, currentDepth + 1, (baseDirectory, currentPath, fileCount, parent) =>
                {
                    return new FileTreeViewModel(baseDirectory, currentPath, fileCount, parent);

                }, progressHandler, searchPatterns);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        #region (private) Data Loaders
        private void LoadGenres(ref LibraryViewModel libraryViewModel, DialogProgressHandler progressHandler)
        {
            var result = new List<GenreViewModel>();

            var genreEntities = _audioStationDbClient.GetEntities<Genre>();
            var genreCount = genreEntities.Count();
            var genreIndex = 0;

            foreach (var genre in genreEntities.OrderBy(x => x.Name))
            {
                libraryViewModel.AddGenre(new GenreViewModel(genre.Id)
                {
                    Name = genre.Name
                });

                // Progress Update
                progressHandler(3, 1, genreCount, ++genreIndex, "Loading Genres...");
            }
        }
        private void LoadArtists(ref LibraryViewModel libraryViewModel, DialogProgressHandler progressHandler)
        {
            // Database:  Load the artist entities
            var artistEntities = _audioStationDbClient.GetEntities<Artist>();
            var artistCount = artistEntities.Count();
            var artistIndex = 0;

            // Load the album collection
            foreach (var artist in artistEntities.OrderBy(x => x.Name))
            {
                // Database:  Load the album entities
                var albums = _audioStationDbClient.GetArtistAlbums(artist.Id, true);

                // Create Artist Result
                var artistViewModel = new ArtistViewModel(artist.Id)
                {
                    Artist = artist.Name
                };

                foreach (var album in albums)
                {
                    // NOTE:  The library must have related unique instances!
                    //

                    // Album
                    libraryViewModel.AddAlbum(new AlbumViewModel(album.Id)
                    {
                        Album = album.Name,
                        Duration = TimeSpan.Zero,        // Need tracks still!
                        PrimaryArtist = artist.Name,
                        Year = album.Year,
                        MediaFormat = album.MediaFormat,
                    });

                    var albumViewModel = libraryViewModel.GetAlbum(album.Id);

                    // Album -> Media
                    for (int mediaNumber = 1; mediaNumber <= album.MediaCount; mediaNumber++)
                    {
                        // Tracks still need to be loaded!
                        albumViewModel.Media.Add(new MediaViewModel()
                        {
                            Duration = TimeSpan.Zero,
                            MediaNumber = mediaNumber
                        });
                    }

                    // Artist -> Albums
                    artistViewModel.Albums.Add(libraryViewModel.GetAlbum(album.Id));
                }

                // Add Artist to Library
                libraryViewModel.AddArtist(artistViewModel);

                // Progress Update
                progressHandler(3, 2, artistCount, ++artistIndex, "Loading Artists...");
            }
        }
        private void LoadTracks(ref LibraryViewModel libraryViewModel, DialogProgressHandler progressHandler)
        {
            var result = new List<TrackViewModel>();

            var trackEntities = _audioStationDbClient.GetEntities<Track>();
            var trackCount = trackEntities.Count();
            var trackIndex = 0;

            foreach (var track in trackEntities)
            {
                var trackViewModel = new TrackViewModel(track.Id, track.AlbumId, track.ArtistId)
                {
                    Album = track.Album.Name,
                    Artist = track.Artist.Name,
                    Duration = TimeSpan.FromMilliseconds(track.DurationMilliseconds),
                    FileName = track.FileReference.FileName,
                    FileCorruptMessage = track.FileReference.FileCorruptMessage,
                    FileLoadErrorMessage = track.FileReference.FileErrorMessage,
                    Crc32 = track.FileReference.CRC32,
                    Genre = track.Genre.Name,
                    IsFileAvailable = !track.FileReference.IsFileLoadError,
                    IsFileCorrupt = track.FileReference.IsFileCorrupt,
                    IsFileLoadError = track.FileReference.IsFileLoadError,
                    MediaFormat = track.Album.MediaFormat,
                    MediaNumber = track.MediaNumber,
                    Title = track.Title,
                    TrackNumber = track.TrackNumber
                };

                // Media -> Tracks
                var media = libraryViewModel.GetAlbum(track.AlbumId)
                                            .Media
                                            .First(x => x.MediaNumber == track.MediaNumber);

                media.Tracks.Add(trackViewModel);
                media.Duration = TimeSpan.FromMilliseconds(media.Tracks.Sum(x => x.Duration.TotalMilliseconds));

                // Album -> Media
                var album = libraryViewModel.GetAlbum(track.AlbumId);

                album.Duration = TimeSpan.FromMilliseconds(media.Duration.TotalMilliseconds);

                // Library Track
                libraryViewModel.AddTrack(trackViewModel);

                // Progress Update
                progressHandler(3, 3, trackCount, ++trackIndex, "Loading Tracks...");
            }
        }
        #endregion
    }
}
