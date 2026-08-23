using System.IO;
using System.Windows.Threading;

using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Component
{
    [IocExport(typeof(IComponentViewModelLoader))]
    public class ComponentViewModelLoader : IComponentViewModelLoader
    {
        private readonly IIocEventAggregator _eventAggregator;

        private readonly ILibraryImporter _libraryImporter;

        private readonly IModelController _modelController;
        private readonly IConfigurationManager _configurationManager;

        private readonly LibraryManagerViewModel _libraryManagerViewModel;
        private readonly RadioViewModel _radioViewModel;
        private readonly LogViewModel _logViewModel;
        private readonly LibraryImporterViewModel _libraryImporterViewModel;
        private readonly LibraryLoaderCDImportViewModel _libraryLoaderCDImportViewModel;

        [IocImportingConstructor]
        public ComponentViewModelLoader(

            IIocEventAggregator eventAggregator,

            // Core Components
            ILibraryImporter libraryImporter,

            // View Models
            LibraryLoaderCDImportViewModel libraryLoaderCDImportViewModel,
            LibraryImporterViewModel libraryImporterViewModel,
            LibraryManagerViewModel libraryManagerViewModel,
            RadioViewModel radioViewModel,
            LogViewModel logViewModel,

            // Controllers
            IModelController modelController,
            IConfigurationManager configurationManager)
        {
            _eventAggregator = eventAggregator;

            _libraryImporter = libraryImporter;

            _libraryLoaderCDImportViewModel = libraryLoaderCDImportViewModel;
            _libraryImporterViewModel = libraryImporterViewModel;
            _libraryManagerViewModel = libraryManagerViewModel;
            _radioViewModel = radioViewModel;
            _logViewModel = logViewModel;

            _modelController = modelController;
            _configurationManager = configurationManager;
        }

        public Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.Run(async () =>
            {
                // Procedure
                //
                // 0) Load / Validation Configuration
                // 1) Load Data (for view models)
                // 2) Initialize View Models
                // 3) Load View Model Data
                //

                var taskCount = 5;
                var task = 0;

                var configuration = _configurationManager.GetConfiguration();

                // Log (first)
                progressHandler(taskCount, task++, 0, "Initializing Log...");
                _logViewModel.Initialize(configuration, LogViewModel_CreateLoad(progressHandler), progressHandler);

                // CD Drive
                progressHandler(taskCount, task++, 0, "Initializing CD Drive...");
                _libraryLoaderCDImportViewModel.Initialize(configuration, new NoViewModel(), progressHandler);

                // Library Importer
                progressHandler(taskCount, task++, 0, "Initializing Library Importer...");
                _libraryImporterViewModel.Initialize(configuration, LoadImporterViewModel_CreateLoad(progressHandler), progressHandler);

                // Library Manager
                progressHandler(taskCount, task++, 0, "Initializing Library Manager...");
                _libraryManagerViewModel.Initialize(configuration, LibraryManagerViewModel_CreateLoad(progressHandler), progressHandler);

                // Radio
                progressHandler(taskCount, task++, 0, "Initializing Radio...");
                _radioViewModel.Initialize(configuration, new NoViewModel(), progressHandler);
            });
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

        public Task ConvertFiles(IEnumerable<string> convertibleFiles, Action<double, string> progressCallback)
        {
            var configuration = _configurationManager.GetConfiguration();

            return Task.Run(() =>
            {
                //try
                //{
                //    var directory = System.IO.Path.Combine(configuration.DownloadFolder, CONVERT_OUTPUT_FOLDER);

                //    if (!Directory.Exists(directory))
                //        Directory.CreateDirectory(directory);

                //    var fileCounter = 0;
                //    var totalFiles = convertibleFiles.Count();

                //    foreach (var filePath in convertibleFiles)
                //    {
                //        var fileName = System.IO.Path.GetFileName(filePath);
                //        var name = System.IO.Path.GetFileNameWithoutExtension(fileName);

                //        // Strip other library folders, and the file name, to get the proper sub-folders
                //        //
                //        var subDirectory = filePath.Replace(configuration.DirectoryBase, string.Empty)
                //                                   .Replace(fileName, string.Empty)
                //                                   .TrimStart('\\')
                //                                   .TrimEnd('\\');

                //        var stagingDirectory = directory.TrimEnd('\\') + "\\" + subDirectory;

                //        if (!Directory.Exists(stagingDirectory))
                //            Directory.CreateDirectory(stagingDirectory);

                //        var destinationFile = System.IO.Path.Combine(stagingDirectory, name + ".mp3");
                //        var success = false;

                //        try
                //        {
                //            // Convert File (into staging)
                //            using (var reader = new MediaFoundationReader(filePath))
                //            {
                //                using (var mp3Writer = new LameMP3FileWriter(destinationFile, reader.WaveFormat, 128))
                //                {
                //                    reader.CopyTo(mp3Writer);
                //                }
                //            }

                //            // Success!
                //            success = true;
                //        }
                //        catch (Exception ex)
                //        {
                //            ApplicationHelpers.Log("Error converting file: {0}, {1}", LogLevel.Error, ex, fileName, ex.Message);
                //        }


                //        // Progress %
                //        progressCallback(++fileCounter / (double)totalFiles, fileName);

                //        // Delete Original File
                //        if (success)
                //            File.Delete(filePath);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    ApplicationHelpers.Log("Error converted files:  {0}", LogLevel.Error, ex, ex.Message);
                //    throw ex;
                //}
            });
        }


        #region (private) ViewModel Load Creators

        private LogSetViewModel LogViewModel_CreateLoad(DialogProgressHandler progressHandler)
        {
            // TODO: Load log from file
            return new LogSetViewModel();
        }

        private LibraryViewModel LibraryManagerViewModel_CreateLoad(DialogProgressHandler progressHandler)
        {
            // Load Searchable Data (except for the library entries)
            try
            {
                var loadViewModel = new LibraryViewModel();

                var artists = LoadArtists(progressHandler);
                var albums = LoadAlbums(progressHandler);
                var genres = LoadGenres(progressHandler);

                BasicHelpers.InvokeDispatcher(() =>
                {
                    loadViewModel.Artists.AddRange(artists);
                    loadViewModel.Albums.AddRange(albums);
                    loadViewModel.Genres.AddRange(genres);

                }, DispatcherPriority.Background);

                return loadViewModel;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error Loading Audio Station Entities:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private LibraryImporterTreeViewModel? LoadImporterViewModel_CreateLoad(DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                return BasicHelpers.InvokeDispatcher(() => { return LoadImporterViewModel_CreateLoad(progressHandler); }, DispatcherPriority.Background);

            else
            {
                // Configuration:  Calculate base directory from staging
                //
                var configuration = _configurationManager.GetConfiguration();
                var options = _libraryImporterViewModel.Options;

                if (string.IsNullOrWhiteSpace(configuration.DirectoryBase) ||
                    string.IsNullOrWhiteSpace(configuration.MusicSubDirectory) ||
                    string.IsNullOrWhiteSpace(configuration.AudioBooksSubDirectory))
                    return null;


                var directoryBase = configuration.DirectoryBase;
                var subDirectory = options.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                                   options.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                                   string.Empty;

                // Calculate Migration (destination) Directory
                var destinationDirectory = System.IO.Path.Combine(directoryBase, subDirectory);

                try
                {
                    // Directory (Root -> NodeValue)
                    var rootValue = new LibraryImporterDirectoryViewModel(options.SourceFolder, options);

                    // File Tree (Recursive Node Container)
                    var root = new LibraryImporterTreeViewModel(rootValue);

                    // Recurse through files using a while loop
                    var directories = new Stack<LibraryImporterTreeViewModel>();

                    // Start (stack)
                    directories.Push(root);

                    while (directories.Count > 0)
                    {
                        var currentDirectory = directories.Pop();

                        // Current Directory
                        var fileData = BasicHelpers.FastGetFileData(currentDirectory.NodeValue.FullPath, "*.mp3", true, SearchOption.TopDirectoryOnly);
                        var fileCount = fileData.Count();
                        var fileIndex = 0;

                        foreach (var file in fileData)
                        {
                            //progressHandler(fileCount, fileIndex++, 0, "Loading Import Files");

                            // Directory (stack)
                            if (file.IsDirectory)
                            {
                                // Next Directory
                                var nodeValue = new LibraryImporterDirectoryViewModel(file.Path, options);

                                // Current -> Next (adds parent)
                                var nextDirectory = currentDirectory.Add(nodeValue) as LibraryImporterTreeViewModel;

                                // Push (NodeValue, Parent)
                                directories.Push(nextDirectory);
                            }

                            else
                                currentDirectory.Add(new LibraryImporterFileViewModel(file.Path, false, destinationDirectory, options));
                        }
                    }

                    return root;
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                    return null;
                }
            }
        }

        #endregion

        #region (public) UI Workflows

        public async Task LibraryImporter_RunAcoustID()
        {
            // Procedure:  File collections are for read-only user data (except for the SourceDirectory recursive tree)
            //
            // 0) Show Dialog (progress handler)
            // 1) Clear file collections from last AcoustID run
            // 2) Run AcoustID on staged files (read-only collection until import is completed)
            //

            var progressCounter = 0;
            var progressTotal = _libraryImporterViewModel.StagedFiles.Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running AcoustID Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            // Clear AcoustID from last run
            _libraryImporterViewModel.AcoustIDCompletedSuccessfully.Clear();

            foreach (var selectedFile in _libraryImporterViewModel.StagedFiles)
            {
                // Double Check (existing results)
                if (!selectedFile.ImportOutput.AcoustIDSuccess)
                {
                    loadingViewModel.Message = "AcoustID: " + selectedFile.ShortPath;

                    await LibraryImporter_RunAcoustID_Impl(selectedFile);

                    // Success
                    if (selectedFile.ImportOutput.AcoustIDSuccess)
                        _libraryImporterViewModel.MusicBrainzCompletedSuccessfully.Add(selectedFile);
                }

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        public async Task LibraryImporter_RunMusicBrainz()
        {
            // Procedure
            //
            // 0) Show Dialog (progress handler)
            // 1) Run Music Brainz on staged files of library importer
            //
            var progressCounter = 0;
            var progressTotal = _libraryImporterViewModel.StagedFiles.Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running Music Brainz Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var selectedFile in _libraryImporterViewModel.StagedFiles)
            {
                // Double Check (existing records)
                //
                if (!selectedFile.ImportOutput.MusicBrainzRecordingMatchSuccess)
                {
                    loadingViewModel.Message = "Music Brainz Lookup:  " + selectedFile.ShortPath;

                    await LibraryImporter_RunMusicBrainz_Impl(selectedFile);
                }

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        public async Task LibraryImporter_RunImport()
        {
            // Procedure
            //
            // 0) Show Dialog (progress handler)
            // 1) Run source files that are staged and verified using ILibraryImporter
            //

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Importing Audio Files",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = true
            };
            var progressCounter = 0;
            var progressTotal = _libraryImporterViewModel.StagedFiles.Count();

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            for (int index = _libraryImporterViewModel.StagedFiles.Count - 1; index >= 0; index--)
            {
                var file = _libraryImporterViewModel.StagedFiles[index];

                loadingViewModel.Message = "Importing " + file.ShortPath;

                await LibraryLoader_RunImport_Impl(file);

                // -> Success Collection
                if (file.ImportOutput.Mp3FileImportSuccess)
                    _libraryImporterViewModel.FilesCompletedSuccessfully.Add(file);

                // -> Error Collection (import (or) migration error)
                else
                    _libraryImporterViewModel.FilesCompletedWithError.Add(file);

                // Remove Staged File (will have to restage error files)
                _libraryImporterViewModel.StagedFiles.RemoveAt(index);

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        private async Task LibraryImporter_RunAcoustID_Impl(LibraryImporterFileViewModel selectedFile)
        {
            // Log (start)
            selectedFile.ImportOutput.LogMessages.Add("AcoustID Chroma-Print Service Started:  " + selectedFile.ShortPath);

            var success = await _libraryImporter.WorkAcoustID(selectedFile.ImportLoad, selectedFile.ImportOutput);

            if (!success)
                selectedFile.ImportOutput.LogMessages.Add("AcoustID Failed (progress halted)");
            else
            {
                selectedFile.ImportOutput.LogMessages.Add("AcoustID Succeeded!");

                // Set initial selection
                selectedFile.SelectedAcoustIDResult = selectedFile.ImportOutput.AcoustIDResults.First();
            }
        }

        private async Task LibraryImporter_RunMusicBrainz_Impl(LibraryImporterFileViewModel selectedFile)
        {
            // Log (start)
            selectedFile.ImportOutput.LogMessages.Add("Music Brainz Lookup Started:  " + selectedFile.ShortPath);

            var success = await _libraryImporter.WorkMusicBrainzDetail(selectedFile.ImportLoad, selectedFile.ImportOutput);

            if (!success)
                selectedFile.ImportOutput.LogMessages.Add("Music Brainz Failed (progress halted)");
            else
            {
                selectedFile.ImportOutput.LogMessages.Add("Music Brainz Succeeded!");

                // Set initial selection
                selectedFile.SelectedMusicBrainzRecordingMatch = selectedFile.ImportOutput.MusicBrainzRecordingMatches.First();
            }
        }

        private async Task LibraryLoader_RunImport_Impl(LibraryImporterFileViewModel selectedFile)
        {
            // Log (start)
            selectedFile.ImportOutput.LogMessages.Add("Import Started:  " + selectedFile.FullPath);

            // -> Import to Database
            var success = _libraryImporter.WorkImportEntity(selectedFile.ImportLoad, selectedFile.ImportOutput);

            if (!success)
                selectedFile.ImportOutput.LogMessages.Add("Import Failed (progress halted)");
            else
                selectedFile.ImportOutput.LogMessages.Add("Import Succeeded!");

            // -> Migrate File
            if (selectedFile.ImportLoad.ImportFileMigration && success)
            {
                if (!_libraryImporter.CanImportMigrateFile(selectedFile.ImportLoad, selectedFile.ImportOutput))
                    selectedFile.ImportOutput.LogMessages.Add("File Migration Not Possible (progress halted)");

                else
                {
                    // Log (migration)
                    selectedFile.ImportOutput.LogMessages.Add("Migrating File:  " + selectedFile.FileMigrationFullPath);

                    var migrationSuccess = _libraryImporter.WorkMigrateFile(selectedFile.ImportLoad, selectedFile.ImportOutput);

                    if (!migrationSuccess)
                        selectedFile.ImportOutput.LogMessages.Add("File Migration Failed (progress halted)");
                    else
                        selectedFile.ImportOutput.LogMessages.Add("File Migration Succeeded!");
                }
            }
        }
        #endregion

        #region (private) Data Loaders

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
                    ApplicationHelpers.Log("Error loading album-artist:  AlbumId={0}", LogLevel.Error, null, albumEntity.Id);
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

        #endregion
    }
}
