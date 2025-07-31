
using System;
using System.IO;
using System.Windows.Shapes;

using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
using AudioStation.ViewModels.LibraryViewModels;

using Microsoft.Extensions.Logging;

using NAudio.Lame;
using NAudio.Wave;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Component
{
    [IocExport(typeof(IViewModelLoader))]
    public class ViewModelLoader : IViewModelLoader
    {
        readonly IModelController _modelController;
        readonly IConfigurationManager _configurationManager;

        private readonly string[] CONVERTIBLE_FILE_EXT;
        private readonly string CONVERT_OUTPUT_FOLDER = "ConvertedFiles";

        [IocImportingConstructor]
        public ViewModelLoader(IModelController modelController, IConfigurationManager configurationManager)
        {
            _modelController = modelController;
            _configurationManager = configurationManager;

            CONVERTIBLE_FILE_EXT = new string[]
            {
                ".wma", ".wav", ".m4a"
            };
        }

        public IEnumerable<ArtistViewModel> LoadArtists(DialogProgressHandler progressHandler)
        {
            var resultCollection = new List<ArtistViewModel>();

            // Database:  Load the artist entities
            var artistEntities = _modelController.GetAudioStationEntities<Mp3FileReferenceArtist>();
            var artistCount = artistEntities.Count();
            var artistIndex = 0;

            // Load the album collection
            foreach (var artist in artistEntities.OrderBy(x => x.Name))
            {
                // Database:  Load the album entities
                var albums = _modelController.GetArtistAlbums(artist.Id, true);

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
                    var tracks = _modelController.GetAlbumTracks(album.Id);

                    // Create tracks for the album
                    albumViewModel.Tracks.AddRange(tracks.Select(MapTrack));

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

            var genreEntities = _modelController.GetAudioStationEntities<Mp3FileReferenceGenre>();
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

            var albumEntities = _modelController.GetAudioStationEntities<Mp3FileReferenceAlbum>();
            var trackEntities = _modelController.GetAudioStationEntities<Mp3FileReference>();

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
                    ApplicationHelpers.Log("Error loading album-artist:  AlbumId={0}", LogMessageType.General, LogLevel.Error, null, albumEntity.Id);
                    continue;
                }

                // Artist Entity
                var artist = _modelController.GetAudioStationEntity<Mp3FileReferenceArtist>((int)artistId);

                // Album Result
                var album = MapAlbum(artist, albumEntity, tracks);

                result.Add(album);

                // Progress Update
                progressHandler(albumCount, ++albumIndex, 0, "Loading Albums...");
            }

            return result;
        }

        public PageResult<LibraryEntryViewModel> LoadEntryPage(PageRequest<Mp3FileReference, int> request)
        {
            var result = new PageResult<LibraryEntryViewModel>();

            // Database:  Load the file (entry) entities
            var entryPage = _modelController.GetAudioStationPage(request);

            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.TotalRecordCountFiltered = entryPage.TotalRecordCountFiltered;
            result.TotalRecordCount = entryPage.TotalRecordCount;
            result.Results = entryPage.Results.Select(MapTrack).ToList();

            return result;
        }

        public LibraryEntryViewModel MapTrack(Mp3FileReference track)
        {
            return new LibraryEntryViewModel(track.Id)
            {
                Album = track.Album?.Name ?? "Unknown",
                Disc = (uint)(track.Album?.DiscNumber ?? 0),
                Duration = TimeSpan.FromMilliseconds(track.DurationMilliseconds ?? 0),
                FileName = track.FileName,
                PrimaryArtist = track.PrimaryArtist?.Name ?? "Unknown",
                PrimaryGenre = track.PrimaryGenre?.Name ?? "Unknown",
                Title = track.Title ?? "Unknown",
                Track = (uint)(track.Track ?? 0),
                FileCorruptMessage = track.FileCorruptMessage ?? "",
                FileLoadErrorMessage = track.FileErrorMessage ?? "",
                IsFileAvailable = track.IsFileAvailable,
                IsFileLoadError = track.IsFileLoadError,
                IsFileCorrupt = track.IsFileCorrupt
            };
        }

        public AlbumViewModel MapAlbum(Mp3FileReferenceArtist primaryArtist, Mp3FileReferenceAlbum albumEntity, IEnumerable<Mp3FileReference> tracks)
        {
            return new AlbumViewModel(albumEntity.Id)
            {
                Album = albumEntity.Name,
                Duration = TimeSpan.FromMilliseconds((double)tracks.Sum(track => track.DurationMilliseconds)),
                PrimaryArtist = primaryArtist.Name,
                Tracks = new SortedObservableCollection<LibraryEntryViewModel>(tracks.Select(MapTrack)),
                Year = (uint)albumEntity.Year
            };
        }

        public IEnumerable<string> LoadNonConvertedFiles()
        {
            var configuration = _configurationManager.GetConfiguration();

            try
            {
                var allFiles = ApplicationHelpers.FastGetFileData(configuration.DirectoryBase, "*.*", false, System.IO.SearchOption.AllDirectories);

                return allFiles.Where(x => CONVERTIBLE_FILE_EXT.Any(z => x.Path.EndsWith(z)))
                               .Select(x => x.Path)
                               .ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error getting non-converted files:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public Task ConvertFiles(IEnumerable<string> convertibleFiles, Action<double, string> progressCallback)
        {
            var configuration = _configurationManager.GetConfiguration();

            return Task.Run(() =>
            {
                try
                {
                    var directory = System.IO.Path.Combine(configuration.DownloadFolder, CONVERT_OUTPUT_FOLDER);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var fileCounter = 0;
                    var totalFiles = convertibleFiles.Count();

                    foreach (var filePath in convertibleFiles)
                    {
                        var fileName = System.IO.Path.GetFileName(filePath);
                        var name = System.IO.Path.GetFileNameWithoutExtension(fileName);

                        // Strip other library folders, and the file name, to get the proper sub-folders
                        //
                        var subDirectory = filePath.Replace(configuration.DirectoryBase, string.Empty)
                                                   .Replace(fileName, string.Empty)
                                                   .TrimStart('\\')
                                                   .TrimEnd('\\');

                        var stagingDirectory = directory.TrimEnd('\\') + "\\" + subDirectory;

                        if (!Directory.Exists(stagingDirectory))
                            Directory.CreateDirectory(stagingDirectory);

                        var destinationFile = System.IO.Path.Combine(stagingDirectory, name + ".mp3");
                        var success = false;

                        try
                        {
                            // Convert File (into staging)
                            using (var reader = new MediaFoundationReader(filePath))
                            {
                                using (var mp3Writer = new LameMP3FileWriter(destinationFile, reader.WaveFormat, 128))
                                {
                                    reader.CopyTo(mp3Writer);
                                }
                            }

                            // Success!
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            ApplicationHelpers.Log("Error converting file: {0}, {1}", LogMessageType.General, LogLevel.Error, ex, fileName, ex.Message);
                        }


                        // Progress %
                        progressCallback(++fileCounter / (double)totalFiles, fileName);

                        // Delete Original File
                        if (success)
                            File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error converted files:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                    throw ex;
                }
            });
        }

        public Task<LibraryLoaderImportTreeViewModel?> LoadImportFiles(LibraryLoaderImportOptionsViewModel options,
                                                                       DialogProgressHandler progressHandler)
        {
            if (!_configurationManager.ValidateConfiguration())
                return Task.FromResult<LibraryLoaderImportTreeViewModel?>(null);

            // Configuration:  Calculate base directory from staging
            //
            var configuration = _configurationManager.GetConfiguration();

            var directoryBase = configuration.DirectoryBase;
            var subDirectory = options.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                               options.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                               string.Empty;

            // Calculate Migration (destination) Directory
            var destinationDirectory = System.IO.Path.Combine(directoryBase, subDirectory);

            return Task.Run(() =>
            {
                try
                {
                    // Directory (Root -> NodeValue)
                    var rootValue = new LibraryLoaderImportDirectoryViewModel(options.SourceFolder, options);

                    // File Tree (Recursive Node Container)
                    var root = new LibraryLoaderImportTreeViewModel(rootValue);

                    // Recurse through files using a while loop
                    var directories = new Stack<LibraryLoaderImportTreeViewModel>();                    

                    // Start (stack)
                    directories.Push(root);

                    while (directories.Count > 0)
                    {
                        var currentDirectory = directories.Pop();

                        // Current Directory
                        var fileData = ApplicationHelpers.FastGetFileData(currentDirectory.NodeValue.Path, "*.mp3", true, SearchOption.TopDirectoryOnly);
                        var fileCount = fileData.Count();
                        var fileIndex = 0;

                        foreach (var file in fileData)
                        {
                            //progressHandler(fileCount, fileIndex++, 0, "Loading Import Files");

                            // Directory (stack)
                            if (file.IsDirectory)
                            {
                                // Next Directory
                                var nodeValue = new LibraryLoaderImportDirectoryViewModel(file.Path, options);

                                // Current -> Next (adds parent)
                                var nextDirectory = currentDirectory.Add(nodeValue) as LibraryLoaderImportTreeViewModel;

                                // Push (NodeValue, Parent)
                                directories.Push(nextDirectory);
                            }

                            else
                                currentDirectory.Add(new LibraryLoaderImportFileViewModel(file.Path, false, destinationDirectory, options));
                        }
                    }

                    return root;
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error loading import files:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                    return null;
                }
            });
        }
    }
}
