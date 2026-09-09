using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Utilities;
using SimpleWpf.Extensions.ObservableCollection;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Service
{
    [IocExport(typeof(ILibraryLoaderService))]
    public class LibraryLoaderService : ILibraryLoaderService
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly ILibraryMapperService _libraryMapperService;

        [IocImportingConstructor]
        public LibraryLoaderService(IAudioStationDbClient audioStationDbClient, ILibraryMapperService libraryMapperService)
        {
            _audioStationDbClient = audioStationDbClient;
            _libraryMapperService = libraryMapperService;
        }

        public LibraryViewModel LoadLibrary(DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            // Load Searchable Data (except for the library entries)
            try
            {
                var loadViewModel = new LibraryViewModel();

                var artists = LoadArtists(progressHandler);
                var albums = LoadAlbums(progressHandler);
                var genres = LoadGenres(progressHandler);

                loadViewModel.Artists.AddRange(artists);
                loadViewModel.Albums.AddRange(albums);
                loadViewModel.Genres.AddRange(genres);

                return loadViewModel;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error Loading Audio Station Entities:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public PageResult<TrackViewModel> LoadEntryPage(PageRequest<Track, int> request)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            var result = new PageResult<TrackViewModel>();

            // Database:  Load the file (entry) entities
            var entryPage = _audioStationDbClient.GetPage(request);

            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.TotalRecordCountFiltered = entryPage.TotalRecordCountFiltered;
            result.TotalRecordCount = entryPage.TotalRecordCount;
            result.Results = entryPage.Results.Select(_libraryMapperService.MapTrack).ToList();

            return result;
        }

        public FileTreeViewModel InitializeImporterTree(
                    string directory,
                    string searchPattern,
                    LibraryImporterConfigurationViewModel importerOptions,
                    DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            try
            {
                // Load first depth of the tree (TODO: Fix showing only the root, instead of starting with the child nodes)
                return DirectoryTreeLoader.Load(directory, searchPattern, -1, directoryNode =>
                {
                    return new FileTreeViewModel(searchPattern, directoryNode);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new FileTreeNodeViewModel(directory, directoryPath, directoryFileCount);

                }, filePath =>
                {
                    return new FileTreeNodeViewModel(directory, filePath, 0);

                }, progressHandler);
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
                        string searchPattern,
                        DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Trying to load library on a non-dispatcher thread is not allowed");

            try
            {
                DirectoryTreeLoader.LoadToDepth(treeRoot, searchPattern, currentDepth + 1, directoryNode =>
                {
                    return new FileTreeViewModel(searchPattern, directoryNode);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new FileTreeNodeViewModel(treeRoot.GetNodeValue().FullPath, directoryPath, directoryFileCount);

                }, filePath =>
                {
                    return new FileTreeNodeViewModel(treeRoot.GetNodeValue().FullPath, filePath, 0);

                }, progressHandler);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        #region (private) Data Loaders

        public IEnumerable<ArtistViewModel> LoadArtists(DialogProgressHandler progressHandler)
        {
            var resultCollection = new List<ArtistViewModel>();

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

                // Add Album - Query Tracks
                foreach (var album in albums)
                {
                    var albumViewModel = new AlbumViewModel(album.Id)
                    {
                        Album = album.Name,
                        PrimaryArtist = artist.Name,
                        Year = (uint)album.Year
                    };

                    // Database:  Load the track entities
                    var tracks = _audioStationDbClient.GetAlbumTracks(album.Id);

                    // Create tracks for the album
                    albumViewModel.Tracks.AddRange(tracks.Select(_libraryMapperService.MapTrack));

                    // Calculate the album duration
                    albumViewModel.Duration = TimeSpan.FromMilliseconds(albumViewModel.Tracks.Sum(track => track.Duration.TotalMilliseconds));

                    artistViewModel.Albums.Add(albumViewModel);
                }

                // Add Artist to result page
                resultCollection.Add(artistViewModel);

                // Progress Update
                progressHandler(artistCount, ++artistIndex, 0, "Loading Artists...");
            }

            return resultCollection;
        }

        public IEnumerable<GenreViewModel> LoadGenres(DialogProgressHandler progressHandler)
        {
            var result = new List<GenreViewModel>();

            var genreEntities = _audioStationDbClient.GetEntities<Genre>();
            var genreCount = genreEntities.Count();
            var genreIndex = 0;

            foreach (var genre in genreEntities.OrderBy(x => x.Name))
            {
                result.Add(new GenreViewModel(genre.Id)
                {
                    Name = genre.Name
                });

                // Progress Update
                progressHandler(genreCount, ++genreIndex, 0, "Loading Genres...");
            }

            return result;
        }

        public IEnumerable<AlbumViewModel> LoadAlbums(DialogProgressHandler progressHandler)
        {
            var result = new List<AlbumViewModel>();

            var albumEntities = _audioStationDbClient.GetEntities<Album>();
            var trackEntities = _audioStationDbClient.GetEntities<Track>();

            var albumCount = albumEntities.Count();
            var albumIndex = 0;

            foreach (var albumEntity in albumEntities.OrderBy(x => x.Name))
            {
                // Track Entities
                var tracks = trackEntities.Where(track => track.AlbumId == albumEntity.Id);

                // Primary Artist Id (TODO!!! MULTIPLE ARTISTS, VARYING PER TRACK!)
                var artistId = tracks.Select(track => track.PrimaryArtistId)
                                     .FirstOrDefault();

                if (artistId == null)
                {
                    ApplicationHelpers.Log("Error loading album-artist:  AlbumId={0}", LogLevel.Error, null, albumEntity.Id);
                    continue;
                }

                // Artist Entity
                var artist = _audioStationDbClient.GetEntity<Artist>((int)artistId);

                // Album Result
                var album = _libraryMapperService.MapAlbum(artist, albumEntity, tracks);

                result.Add(album);

                // Progress Update
                progressHandler(albumCount, ++albumIndex, 0, "Loading Albums...");
            }

            return result;
        }

        #endregion
    }
}
