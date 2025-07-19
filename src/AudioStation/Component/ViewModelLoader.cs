
using System.IO;

using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryViewModels;

using Microsoft.Extensions.Logging;

using NAudio.Lame;
using NAudio.MediaFoundation;
using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.NativeIO.FastDirectory;

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

        public PageResult<ArtistViewModel> LoadArtistPage(PageRequest<Mp3FileReferenceArtist, string> request)
        {
            var result = new PageResult<ArtistViewModel>();
            var resultCollection = new List<ArtistViewModel>();

            // Database:  Load the artist entities
            var artistPage = _modelController.GetAudioStationPage(request);

            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.TotalRecordCountFiltered = artistPage.TotalRecordCountFiltered;
            result.TotalRecordCount = artistPage.TotalRecordCount;

            // Load the album collection
            foreach (var artist in artistPage.Results)
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
                    albumViewModel.Tracks.AddRange(tracks.Select(track =>
                    {
                        return new LibraryEntryViewModel(track.Id)
                        {
                            Album = album.Name,
                            Disc = (uint)album.DiscNumber,
                            FileName = track.FileName,
                            PrimaryArtist = artist.Name,
                            Title = track.Title ?? "Unknown",
                            Track = (uint)(track.Track ?? 0),
                            Duration = TimeSpan.FromMilliseconds(track.DurationMilliseconds ?? 0),
                            FileCorruptMessage = track.FileCorruptMessage ?? "",
                            FileLoadErrorMessage = track.FileErrorMessage ?? "",
                            IsFileAvailable = track.IsFileAvailable,
                            IsFileCorrupt = track.IsFileCorrupt,
                            IsFileLoadError = track.IsFileLoadError,
                            PrimaryGenre = track.PrimaryGenre?.Name ?? "Unknown"
                        };
                    }));

                    // Calculate the album duration
                    albumViewModel.Duration = TimeSpan.FromMilliseconds(albumViewModel.Tracks.Sum(track => track.Duration.TotalMilliseconds));

                    artistViewModel.Albums.Add(albumViewModel);
                }

                // Add Artist to result page
                resultCollection.Add(artistViewModel);
            }

            result.Results = resultCollection;

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
            result.Results = entryPage.Results.Select(entry => new LibraryEntryViewModel(entry.Id)
            {
                Album = entry.Album?.Name ?? "Unknown",
                Disc = (uint)(entry.Album?.DiscNumber ?? 0),
                Duration = TimeSpan.FromMilliseconds(entry.DurationMilliseconds ?? 0),
                FileName = entry.FileName,
                PrimaryArtist = entry.PrimaryArtist?.Name ?? "Unknown",
                PrimaryGenre = entry.PrimaryGenre?.Name ?? "Unknown",
                Title = entry.Title ?? "Unknown",
                Track = (uint)(entry.Track ?? 0),
                FileCorruptMessage = entry.FileCorruptMessage ?? "",
                FileLoadErrorMessage = entry.FileErrorMessage ?? "",
                IsFileAvailable = entry.IsFileAvailable,
                IsFileLoadError = entry.IsFileLoadError,
                IsFileCorrupt = entry.IsFileCorrupt
            }).ToList();

            return result;
        }

        public IEnumerable<string> LoadNonConvertedFiles()
        {
            var configuration = _configurationManager.GetConfiguration();

            try
            {
                var allFiles = FastDirectoryEnumerator.GetFiles(configuration.DirectoryBase, "*.*", System.IO.SearchOption.AllDirectories);

                return allFiles.Where(x =>  CONVERTIBLE_FILE_EXT.Any(z => x.Path.EndsWith(z)))
                               .Select(x => x.Path)
                               .ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error getting non-converted files:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
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
                    var directory = Path.Combine(configuration.DownloadFolder, CONVERT_OUTPUT_FOLDER);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var fileCounter = 0;
                    var totalFiles = convertibleFiles.Count();

                    foreach (var filePath in convertibleFiles)
                    {
                        var fileName = Path.GetFileName(filePath);
                        var name = Path.GetFileNameWithoutExtension(fileName);

                        // Strip other library folders, and the file name, to get the proper sub-folders
                        //
                        var subDirectory = filePath.Replace(configuration.DirectoryBase, string.Empty)
                                                   .Replace(fileName, string.Empty)
                                                   .TrimStart('\\')
                                                   .TrimEnd('\\');

                        var stagingDirectory = directory.TrimEnd('\\') + "\\" + subDirectory;

                        if (!Directory.Exists(stagingDirectory))
                            Directory.CreateDirectory(stagingDirectory);

                        var destinationFile = Path.Combine(stagingDirectory, name + ".mp3");
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
                            ApplicationHelpers.Log("Error converting file: {0}, {1}", LogMessageType.General, LogLevel.Error, fileName, ex.Message);
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
                    ApplicationHelpers.Log("Error converted files:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                    throw ex;
                }
            });
        }
    }
}
