using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderImportWorker : LibraryLoaderWorker<LibraryLoaderImportPayload, LibraryLoaderImportOutputPayload>
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IAudioStationFileService _fileController;
        private readonly ITagCache _tagCache;
        private readonly IAudioConverter _audioConverter;

        private const int WORK_STEPS = 6;

        private string _destinationPath;
        private bool _migrationRequired;

        public LibraryLoaderImportWorker(LibraryLoaderWorkItem workItem,
                                         IAudioStationDbClient audioStationDbClient,
                                         IAudioStationFileService fileController,
                                         ITagCache tagCacheController,
                                         IAudioConverter audioConverter)
            : base(workItem)
        {
            _fileController = fileController;
            _audioStationDbClient = audioStationDbClient;
            _tagCache = tagCacheController;
            _audioConverter = audioConverter;

            _destinationPath = string.Empty;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }
        public static int GetNumberSteps()
        {
            return WORK_STEPS;
        }

        protected override LibraryWorkerStepResult Work(int workStep)
        {
            // Steps: Several small steps to ensure that file operations are tracked
            //
            // 1) Calculate File and Folder Paths
            // 2) Validate Import Records:  Source Tag Entity (minimum valid); Source File Reference; All File / Folder Paths (check permissions)
            //
            // 3) Source -> Destination / Convert Source File (optional)
            //      -> File Conversion (optional) requires that you convert the file, apply the proper extension
            //         and delete the source file. The library directory must not be marked read-only.
            //
            // 4) Embed Tag Data (protect read-only option and report if necessary)
            // 5) Import Entity
            // 6) Migrate (optional) / Delete Source File (optional) / Empty Folder (optional)
            //      -> Handling the source file is optional. Migration implies that you're moving
            //         a file from source to destination. "In Place" implies that you're leaving the
            //         file in the same directory. These settings have already been applied.
            // 

            switch (workStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    return CalculateFilePaths(workStep);
                }
                case 2:
                {
                    return ValidateImportRecords(workStep);
                }
                case 3:
                {
                    return SourceToDestination(workStep);
                }
                case 4:
                {
                    return OptionEmbedTagData(workStep);
                }
                case 5:
                {
                    return ImportDatabaseRecords(workStep);
                }
                case 6:
                {
                    return CompleteImport(workStep);
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
            }
        }

        private LibraryWorkerStepResult CalculateFilePaths(int stepNumber)
        {
            try
            {
                Log("Calculating file / folder paths");

                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;

                Log("Retrieving database record for tag data:  Id=" + workLoad.TagSmallId);

                var entity = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);

                if (entity == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Database record for tag not found:  Id=" + workLoad.TagSmallId,
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber,
                    };
                }

                Log("Validating tag data");
                var validation = TagValidator.ValidateTagSmallImport(entity);

                if (!validation.IsValid)
                {
                    Log("Validation error:  " + validation.ValidationMessage);
                }
                else
                {
                    Log("Calculating file name based on preferences and tag information");

                    // Calculate standard file name for the import
                    _destinationPath = _fileController.CalculateGivenFileName(
                        workLoad.SourceFullPath,
                        workLoad.DestinationFolder,
                        workLoad.TrackCategory,
                        entity.Genre,
                        entity.AlbumArtist,
                        entity.Album,
                        entity.Title,
                        entity.TrackNumber ?? 0,
                        entity.TrackTotal ?? 0,
                        true);

                    _migrationRequired = _destinationPath != workLoad.SourceFullPath;

                    Log("Calculation complete:  " + _destinationPath);
                }

                return LibraryWorkerStepResult.Success(stepNumber, "Import file validation successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error calculating file paths:  " + ex.Message);
            }
        }
        private LibraryWorkerStepResult ValidateImportRecords(int stepNumber)
        {
            try
            {
                Log("Validating import records");

                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;

                Log("Retrieving file tag data from source file");

                var tagData = _tagCache.Get(workLoad.SourceFullPath);

                if (tagData == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Error reading tag data from source file",
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber,
                    };
                }

                Log("Retrieving database records for tag:  Id=" + workLoad.TagSmallId);

                var tag = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);
                var tagMap = tag != null ? _audioStationDbClient.FirstEntity<TagSmallFileReferenceMap>(x => x.TagSmallId == tag.Id) : null;
                var track = tagMap != null ? _audioStationDbClient.FirstEntity<Track>(x => x.FileReferenceId == tagMap.FileReferenceId) : null;

                // Show User:  Album, Artist, Track, Genre, FileReference
                if (tagMap != null && tag != null && track != null)
                {
                    Log("WARNING:       Existing Tag Information");
                    Log("Track:         Tag=({0})  Existing=({1})", tag.Title ?? string.Empty, track.Title ?? string.Empty);
                    Log("Album:         Tag=({0})  Existing=({1})", tag.Album ?? string.Empty, track.Album?.Name ?? string.Empty);
                    Log("Artist:        Tag=({0})  Existing=({1})", tag.AlbumArtist ?? string.Empty, track.PrimaryArtist?.Name ?? string.Empty);
                    Log("Genre:         Tag=({0})  Existing=({1})", tag.Genre ?? string.Empty, track.PrimaryGenre?.Name ?? string.Empty);
                    Log("File (ref):    Tag=({0})  Existing=({1})", workLoad.SourceFullPath, track.FileReference?.FileName ?? string.Empty);
                }
                else if (track != null)
                {
                    Log("WARNING:  Records indicate that there has been a previous import for this tag");
                    Log("WARNING:  Record will be overwritten with new tag information");
                }

                // Validate Destination Directory
                var destinationDirectory = Path.GetDirectoryName(_destinationPath);

                // File Move
                if (workLoad.SourceFullPath != _destinationPath)
                {
                    if (!_fileController.CanWriteToPath(_destinationPath))
                    {
                        Log("ERROR:  Cannot write to destination path:  {0}", _destinationPath);
                        Log("ERROR:  Please check import parameters and re-try");

                        return new LibraryWorkerStepResult()
                        {
                            Completed = false,
                            Message = "Cannot write to destination path:  " + _destinationPath,
                            Result = LibraryWorkerResultLevel.DataError,
                            StepNumber = stepNumber,
                        };
                    }
                    else
                        Log("File migration valid based on library settings");
                }

                // File (In Place)
                else
                {
                    Log("File migration (copy / move / delete) not required");
                }

                return LibraryWorkerStepResult.Success(stepNumber, "Import record validation successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error validating import records:  " + ex.Message);
            }
        }
        private LibraryWorkerStepResult SourceToDestination(int stepNumber)
        {
            try
            {
                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;

                if (!_migrationRequired)
                    Log("File migration not required (in place import)");

                else
                {
                    Log("Copying (source) -> (destination)");
                    Log("Source:  " + workLoad.SourceFullPath);
                    Log("Destination:  " + _destinationPath);

                    // -> Copy (source, dest, overwrite)
                    _fileController.CopyFileTo(workLoad.SourceFullPath, _destinationPath, workLoad.MigrationOverwriteDestinationFiles);

                    Log("File copy successful");
                }

                // File Conversion
                if (workLoad.ConvertAudioFormat)
                {
                    Log("OPTION: File Conversion (checking source / destination format(s))");

                    // Get Audio Format (performance will be slower than just checking file extension)
                    var audioEncoding = _audioConverter.GetAudioEncoding(workLoad.SourceFullPath);

                    // Convert
                    if (audioEncoding != workLoad.ImportFormat.Encoding)
                    {
                        // Calculate new destination path
                        var nextDestinationPath = Path.GetFileNameWithoutExtension(_destinationPath) + workLoad.ImportFormat.Extension;

                        Log("Converting file to format:  " + workLoad.ImportFormat.Name);

                        // Try Conversion (let it fail if it must)
                        _audioConverter.ConvertTo(_destinationPath, nextDestinationPath, workLoad.ImportFormat);

                        Log("File conversion successful:  " + nextDestinationPath);
                        Log("Deleting original file:  " + _destinationPath);

                        _fileController.DeleteFile(_destinationPath);

                        _destinationPath = nextDestinationPath;
                    }
                    else
                    {
                        Log("File conversion not required:  Format=" + workLoad.ImportFormat.Name);
                    }

                }

                return LibraryWorkerStepResult.Success(stepNumber, "File confirmation successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error confirming (source) -> (destination) file:  " + ex.Message);
            }
        }
        private LibraryWorkerStepResult OptionEmbedTagData(int stepNumber)
        {
            try
            {
                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;

                if (!workLoad.EmbedImportTagData)
                {
                    return LibraryWorkerStepResult.Success(stepNumber, "Import file tag embedding option not selected");
                }

                var tag = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);
                var tagData = _tagCache.Get(_destinationPath);

                if (tagData == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Error retrieving tag information from source file",
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber,
                    };
                }
                if (tag == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Error retrieving working tag information from database",
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber,
                    };
                }

                //tagData.Album = tag.Album;
                //tagData.AlbumArtist = tag.AlbumArtist;
                //tagData.Artist = tag.AlbumArtist;
                //tagData.DiscNumber = (ushort)(tag.MediaNumber ?? 0);
                //tagData.DiscTotal = (ushort)(tag.MediaTotal ?? 0);
                //tagData.Duration = TimeSpan.FromMilliseconds(tag.DurationMilliseconds ?? 0);
                //tagData.Genre = tag.Genre ?? string.Empty;
                //tagData.MediaFormat = tag.MediaFormat ?? string.Empty;
                //tagData.Title = tag.Title ?? string.Empty;
                //tagData.Track = (uint)(tag.TrackNumber ?? 0);
                //tagData.TrackTotal = (ushort)(tag.TrackTotal ?? 0);
                //tagData.Year = (int)(tag.Year ?? 0);

                // Validation
                var validation = TagValidator.ValidateTagImport(tagData);

                if (!validation.IsValid)
                {
                    Log("Tag information is invalid (from destination file):  " + _destinationPath);
                    Log("VALIDATION MESSAGE:  " + validation.ValidationMessage);
                    Log("ERROR:  Please contact Audio Station support (or check file permissions for your directory)");

                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Tag Information Invalid: " + validation.ValidationMessage,
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber,
                    };
                }

                Log("Embedding tag data from records");

                // -> Save
                _tagCache.SetData(_destinationPath, tagData);

                Log("Tag information saved:  " + _destinationPath);

                return LibraryWorkerStepResult.Success(stepNumber, "Import tag data embedding successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error embedding tag data:  " + ex.Message);
            }
        }
        private LibraryWorkerStepResult ImportDatabaseRecords(int stepNumber)
        {
            try
            {
                Log("Importing library database records");

                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;
                var tag = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);

                if (tag == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Tag database records missing for the import!Please retry after completing your import.",
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                var validation = TagValidator.ValidateTagSmallImport(tag);

                if (!validation.IsValid)
                {
                    Log("Tag information is invalid (from the database record):  " + tag.Id);
                    Log("VALIDATION MESSAGE:  " + validation.ValidationMessage);

                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Tag information invalid: " + validation.ValidationMessage,
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                var tagMap = _audioStationDbClient.FirstEntity<TagSmallFileReferenceMap>(x => x.TagSmallId == workLoad.TagSmallId);
                var vendorMap = _audioStationDbClient.FirstEntity<TagSmallVendorMap>(x => x.TagSmallId == tag.Id);
                var fileRef = tagMap != null ? tagMap.FileReference : null;
                var fileRefExisting = _audioStationDbClient.FirstEntity<FileReference>(x => x.FileName == _destinationPath);
                var genre = _audioStationDbClient.FirstEntity<Genre>(x => x.Name == tag.Genre);
                var artist = _audioStationDbClient.FirstEntity<Artist>(x => x.Name == tag.AlbumArtist);
                var album = _audioStationDbClient.FirstEntity<Album>(x => x.Name == tag.Album);
                var track = _audioStationDbClient.FirstEntity<Track>(x => x.Title == tag.Title);

                // Library Conflicts
                //

                // FileReference
                if ((fileRef != null || fileRefExisting != null) && !workLoad.LibraryOverwriteExistingFiles)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = string.Format("Library (File) Conflict Found: Id={0}, File={1} ", fileRef?.FileName ?? fileRefExisting?.FileName),
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                // Genre
                if (genre != null && !workLoad.LibraryOverwriteExistingGenres)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = string.Format("Library (Genre) Conflict Found: Id={0}, Name={1} ", genre.Id, genre.Name),
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                // Artist
                if (artist != null && !workLoad.LibraryOverwriteExistingArtists)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = string.Format("Library (Artist) Conflict Found: Id={0}, Name={1} ", artist.Id, artist.Name),
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                // Album
                if (album != null && !workLoad.LibraryOverwriteExistingAlbums)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = string.Format("Library (Album) Conflict Found: Id={0}, Name={1} ", album.Id, album.Name),
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                // Track
                if (track != null && !workLoad.LibraryOverwriteExistingTracks)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = string.Format("Library (Track) Conflict Found: Id={0}, Name={1} ", track.Id, track.Title),
                        Result = LibraryWorkerResultLevel.DataError,
                        StepNumber = stepNumber
                    };
                }

                // New File
                if (tagMap == null)
                {
                    fileRef = new FileReference()
                    {
                        CRC32 = 0,
                        Created = DateTime.Now.ToUniversalTime(),
                        FileCorruptMessage = null,
                        FileErrorMessage = null,
                        FileName = _destinationPath,
                        IsFileAvailable = true,
                        IsFileCorrupt = false,
                        IsFileLoadError = false,
                        LastModified = DateTime.Now.ToUniversalTime()
                    };

                    AddEntity(fileRef, "File");

                    tagMap = new TagSmallFileReferenceMap()
                    {
                        TagSmallId = workLoad.TagSmallId,
                        FileReference = fileRef
                    };

                    AddEntity(fileRef, "File Reference Map");
                }
                else // Foreign Key (FileReference)
                {
                    fileRef.CRC32 = 0;
                    fileRef.Created = DateTime.Now.ToUniversalTime();
                    fileRef.FileCorruptMessage = null;
                    fileRef.FileErrorMessage = null;
                    fileRef.FileName = _destinationPath;
                    fileRef.IsFileAvailable = true;
                    fileRef.IsFileCorrupt = false;
                    fileRef.IsFileLoadError = false;
                    fileRef.LastModified = DateTime.Now.ToUniversalTime();

                    UpdateEntity(fileRef, "File Reference Map");
                }

                // Genre
                if (genre == null)
                {
                    genre = new Genre() { Name = tag.Genre };

                    AddEntity(genre, "Genre");
                }

                // Artist
                if (artist == null)
                {
                    artist = new Artist() { Name = tag.AlbumArtist };

                    AddEntity(artist, "Artist");
                }

                // Album
                if (album == null)
                {
                    album = new Album()
                    {
                        Name = tag.Album,
                        MediaCount = tag.MediaTotal,
                        MediaFormat = tag.MediaFormat,
                        MediaNumber = tag.MediaNumber,
                        Year = tag.Year
                    };

                    AddEntity(album, "Album");
                }
                else
                {
                    album.MediaCount = tag.MediaTotal;
                    album.MediaFormat = tag.MediaFormat;
                    album.MediaNumber = tag.MediaNumber;
                    album.Name = tag.Album;
                    album.Year = tag.Year;

                    UpdateEntity(album, "Album");
                }

                // Track
                if (track == null)
                {
                    track = new Track()
                    {
                        AlbumId = album.Id,
                        DurationMilliseconds = tag.DurationMilliseconds,
                        FileReferenceId = fileRef.Id,
                        Number = tag.MediaNumber,
                        PrimaryArtistId = artist.Id,
                        PrimaryGenreId = genre.Id,
                        Title = tag.Title
                    };

                    AddEntity(track, "Track");

                    var trackArtistMap = new TrackArtistMap()
                    {
                        ArtistId = artist.Id,
                        IsPrimaryArtist = true,
                        TrackId = track.Id,
                    };

                    AddEntity(trackArtistMap, "Track Artist Map");

                    var trackGenreMap = new TrackGenreMap()
                    {
                        GenreId = genre.Id,
                        IsPrimaryGenre = true,
                        TrackId = track.Id,
                    };

                    AddEntity(trackGenreMap, "Track Genre Map");
                }
                else
                {
                    track.AlbumId = album.Id;
                    track.DurationMilliseconds = tag.DurationMilliseconds;
                    track.FileReferenceId = fileRef.Id;
                    track.Number = tag.MediaNumber;
                    track.PrimaryArtistId = artist.Id;
                    track.PrimaryGenreId = genre.Id;
                    track.Title = tag.Title;

                    UpdateEntity(track, "Track");

                    var trackArtistMap = _audioStationDbClient.FirstEntity<TrackArtistMap>(x => x.TrackId == track.Id);
                    var trackGenreMap = _audioStationDbClient.FirstEntity<TrackGenreMap>(x => x.TrackId == track.Id);

                    if (trackArtistMap == null)
                    {
                        trackArtistMap = new TrackArtistMap()
                        {
                            ArtistId = artist.Id,
                            IsPrimaryArtist = true,
                            TrackId = track.Id,
                        };

                        AddEntity(trackArtistMap, "Track Artist Map");
                    }
                    else
                    {
                        trackArtistMap.TrackId = track.Id;
                        trackArtistMap.ArtistId = artist.Id;
                        trackArtistMap.IsPrimaryArtist = true;

                        UpdateEntity(trackArtistMap, "Track Artist Map");
                    }

                    if (trackGenreMap == null)
                    {
                        trackGenreMap = new TrackGenreMap()
                        {
                            GenreId = genre.Id,
                            IsPrimaryGenre = true,
                            TrackId = track.Id,
                        };

                        AddEntity(trackGenreMap, "Track Genre Map");
                    }
                    else
                    {
                        trackGenreMap.TrackId = track.Id;
                        trackGenreMap.GenreId = artist.Id;
                        trackGenreMap.IsPrimaryGenre = true;

                        UpdateEntity(trackGenreMap, "Track Genre Map");
                    }

                    workOutput.TrackGenreMapId = trackGenreMap.Id;
                    workOutput.TrackArtistMapId = trackArtistMap.Id;
                }

                // Reference Database Id's
                workOutput.TagSmallId = tag.Id;
                workOutput.TagSmallFileReferenceMapId = tagMap.Id;
                workOutput.TagSmallVendorMapId = vendorMap.Id;
                workOutput.FileReferenceId = fileRef.Id;
                workOutput.GenreId = genre.Id;
                workOutput.ArtistId = artist.Id;
                workOutput.AlbumId = album.Id;
                workOutput.TrackId = track.Id;

                return LibraryWorkerStepResult.Success(stepNumber, "Import of database records for new track successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error embedding tag data:  " + ex.Message);
            }
        }
        private LibraryWorkerStepResult CompleteImport(int stepNumber)
        {
            try
            {
                var workLoad = this.Load.Payload;
                var workOutput = this.Output.Payload;

                Log("Completing import...");

                if (_migrationRequired)
                {
                    Log("Completing file migration");

                    // Delete Source File(s)
                    if (workLoad.MigrationDeleteSourceFiles)
                    {
                        Log("MIGRATION:  Deleting Source File: " + workLoad.SourceFullPath);

                        _fileController.DeleteFile(workLoad.SourceFullPath);

                        Log("MIGRATION:  Deleting Source File Successful!");
                    }

                    // Delete Source (Empty) Directory
                    if (workLoad.MigrationDeleteSourceFolders && Directory.GetFiles(workLoad.MigrationSourceDirectory).Length == 0)
                    {
                        Log("MIGRATION:  Deleting (Empty) Source Folder: " + workLoad.MigrationSourceDirectory);

                        _fileController.DeleteEmptyDirectory(workLoad.MigrationSourceDirectory);
                    }

                    Log("File migration complete:  " + _destinationPath);
                }

                // Entity Report
                Log("Database entities added / updated for import");

                Log("Audio Station (TagSmall):                      Id=({0})", workOutput.TagSmallId);
                Log("Audio Station (TagSmallFileReferenceMap):      Id=({0})", workOutput.TagSmallFileReferenceMapId);
                Log("Audio Station (TagSmallVendorMap):             Id=({0})", workOutput.TagSmallVendorMapId);
                Log("Audio Station (FileReference):                 Id=({0})", workOutput.FileReferenceId);
                Log("Audio Station (Genre):                         Id=({0})", workOutput.GenreId);
                Log("Audio Station (Artist):                        Id=({0})", workOutput.ArtistId);
                Log("Audio Station (Album):                         Id=({0})", workOutput.AlbumId);
                Log("Audio Station (Track):                         Id=({0})", workOutput.TrackId);
                Log("Audio Station (TrackGenreMap):                 Id=({0})", workOutput.TrackGenreMapId);
                Log("Audio Station (TrackArtistMap):                Id=({0})", workOutput.TrackArtistMapId);

                return LibraryWorkerStepResult.Success(stepNumber, "Import Process Complete!");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error finishing up import migration:  " + ex.Message);
            }
        }

        private void AddEntity<T>(T entity, string entityName) where T : AudioStationEntityBase
        {
            Log("Adding {0} to the Audio Station database", entityName);

            _audioStationDbClient.AddEntity(entity);

            Log("{0} added successfully:  Id=({1})", entityName, entity.Id);
        }

        private void UpdateEntity<T>(T entity, string entityName) where T : AudioStationEntityBase
        {
            Log("Updating {0} in the Audio Station database:  Id=({1})", entityName, entity.Id);

            _audioStationDbClient.UpdateEntity(entity);

            Log("{0} added successfully:  Id=({1})", entityName, entity.Id);
        }
    }
}
