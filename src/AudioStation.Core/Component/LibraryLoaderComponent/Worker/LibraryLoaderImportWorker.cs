using System.IO;

using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Utility;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderImportWorker : LibraryLoaderWorker
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IFileController _fileController;
        private readonly ITagCacheController _tagCacheController;

        private const int WORK_STEPS = 6;

        private string _destinationPath;
        private bool _migrationRequired;

        // Saved entity references
        int _tagSmallId;
        int _tagSmallFileReferenceMapId;
        int _tagSmallVendorMapId;
        int _fileReferenceId;
        int _genreId;
        int _artistId;
        int _albumId;
        int _trackId;
        int _trackGenreMapId;
        int _trackArtistMapId;

        public LibraryLoaderImportWorker(LibraryLoaderWorkItem workItem,
                                         IAudioStationDbClient audioStationDbClient,
                                         IFileController fileController,
                                         ITagCacheController tagCacheController)
            : base(workItem)
        {
            _fileController = fileController;
            _audioStationDbClient = audioStationDbClient;
            _tagCacheController = tagCacheController;

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

        protected override bool Work(int workStep, ref string message)
        {
            // Steps: Several small steps to ensure that file operations are tracked
            //
            // 1) Calculate File and Folder Paths
            // 2) Validate Import Records:  Source Tag Entity (minimum valid); Source File Reference; All File / Folder Paths (check permissions)
            // 3) Copy Source -> Destination
            // 4) Embed Tag Data (protect read-only option and report if necessary)
            // 5) Import Entity
            // 6) Delete Source File / Empty Folder (optional)
            // 

            var load = this.Load.Get<LibraryLoaderImportLoad>();
            var output = this.Output.Get<LibraryLoaderImportOutput>();

            switch (workStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    message = "Calculating file / folder paths";
                    return CalculateFilePaths();
                }
                case 2:
                {
                    message = "Validating import records";
                    return ValidateImportRecords();
                }
                case 3:
                {
                    message = "Copying source file to destination";
                    return CopySourceToDestination();
                }
                case 4:
                {
                    message = "Embedding tag data to destination file";
                    return EmbedTagData();
                }
                case 5:
                {
                    message = "Importing library database records";
                    return ImportDatabaseRecords();
                }
                case 6:
                {
                    message = "Completing migration...";
                    return FinishUpMigration();
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
            }
        }

        private bool CalculateFilePaths()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();

                Log("Retrieving database record for tag data:  Id=" + workLoad.TagSmallId);

                var entity = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);

                if (entity == null)
                {
                    Log("Database record for tag not found:  Id=" + workLoad.TagSmallId);
                    return false;
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
            }
            catch (Exception ex)
            {
                Log("Error calculating file paths:  " + ex.Message);
                return false;
            }

            return true;
        }
        private bool ValidateImportRecords()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();

                Log("Retrieving file tag data from source file");

                var tagData = _tagCacheController.Get(workLoad.SourceFullPath);

                if (tagData == null)
                {
                    Log("Error reading tag data from source file");
                    return false;
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

                        return false;
                    }
                    else
                        Log("File migration valid based on library settings");
                }

                // File (In Place)
                else
                {
                    Log("File migration (copy / move / delete) not required");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log("Error validating import records:  " + ex.Message);
                return false;
            }
        }
        private bool CopySourceToDestination()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();

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

                return true;
            }
            catch (Exception ex)
            {
                Log("Error trying to copy (source) -> (destination):  " + ex.Message, ex);
                return false;
            }
        }
        private bool EmbedTagData()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();
                var tag = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);
                var tagData = _tagCacheController.Get(_destinationPath);

                if (tagData == null)
                {
                    Log("Error retrieving tag information from source file");
                    return false;
                }
                if (tag == null)
                {
                    Log("Error retrieving working tag information from database");
                    return false;
                }

                tagData.Album = tag.Album;
                tagData.AlbumArtist = tag.AlbumArtist;
                tagData.Artist = tag.AlbumArtist;
                tagData.DiscNumber = (ushort)(tag.MediaNumber ?? 0);
                tagData.DiscTotal = (ushort)(tag.MediaTotal ?? 0);
                tagData.Duration = TimeSpan.FromMilliseconds(tag.DurationMilliseconds ?? 0);
                tagData.Genre = tag.Genre ?? string.Empty;
                tagData.MediaFormat = tag.MediaFormat ?? string.Empty;
                tagData.Title = tag.Title ?? string.Empty;
                tagData.Track = (uint)(tag.TrackNumber ?? 0);
                tagData.TrackTotal = (ushort)(tag.TrackTotal ?? 0);
                tagData.Year = (int)(tag.Year ?? 0);

                // Validation
                var validation = TagValidator.ValidateTagImport(tagData);

                if (!validation.IsValid)
                {
                    Log("Tag information is invalid (from destination file):  " + _destinationPath);
                    Log("VALIDATION MESSAGE:  " + validation.ValidationMessage);
                    Log("ERROR:  Please contact Audio Station support (or check file permissions for your directory)");
                    return false;
                }

                Log("Embedding tag data from records");

                // -> Save
                _tagCacheController.SetData(_destinationPath, tagData);

                Log("Tag information saved:  " + _destinationPath);

                return true;
            }
            catch (Exception ex)
            {
                Log("Error embedding tag data:  " + ex.Message, ex);
                return false;
            }
        }
        private bool ImportDatabaseRecords()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();
                var tag = _audioStationDbClient.GetEntity<TagSmall>(workLoad.TagSmallId);

                if (tag == null)
                {
                    Log("Tag database records missing for the import! Please retry after completing your import.");
                    return false;
                }

                var validation = TagValidator.ValidateTagSmallImport(tag);

                if (!validation.IsValid)
                {
                    Log("Tag information is invalid (from the database record):  " + tag.Id);
                    Log("VALIDATION MESSAGE:  " + validation.ValidationMessage);
                    return false;
                }

                var tagMap = _audioStationDbClient.FirstEntity<TagSmallFileReferenceMap>(x => x.TagSmallId == workLoad.TagSmallId);
                var vendorMap = _audioStationDbClient.FirstEntity<TagSmallVendorMap>(x => x.TagSmallId == tag.Id);
                var fileRef = tagMap != null ? tagMap.FileReference : null;
                var genre = _audioStationDbClient.FirstEntity<Genre>(x => x.Name == tag.Genre);
                var artist = _audioStationDbClient.FirstEntity<Artist>(x => x.Name == tag.AlbumArtist);
                var album = _audioStationDbClient.FirstEntity<Album>(x => x.Name == tag.Album);
                var track = _audioStationDbClient.FirstEntity<Track>(x => x.Title == tag.Title);

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
                else
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

                    _trackGenreMapId = trackGenreMap.Id;
                    _trackArtistMapId = trackArtistMap.Id;
                }

                // Reference Database Id's
                _tagSmallId = tag.Id;
                _tagSmallFileReferenceMapId = tagMap.Id;
                _tagSmallVendorMapId = vendorMap.Id;
                _fileReferenceId = fileRef.Id;
                _genreId = genre.Id;
                _artistId = artist.Id;
                _albumId = album.Id;
                _trackId = track.Id;

                return true;
            }
            catch (Exception ex)
            {
                Log("Error embedding tag data:  " + ex.Message, ex);
                return false;
            }
        }
        private bool FinishUpMigration()
        {
            try
            {
                var workLoad = this.Load.Get<LibraryLoaderImportLoad>();

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

                Log("Audio Station (TagSmall):                      Id=({0})", _tagSmallId);
                Log("Audio Station (TagSmallFileReferenceMap):      Id=({0})", _tagSmallFileReferenceMapId);
                Log("Audio Station (TagSmallVendorMap):             Id=({0})", _tagSmallVendorMapId);
                Log("Audio Station (FileReference):                 Id=({0})", _fileReferenceId);
                Log("Audio Station (Genre):                         Id=({0})", _genreId);
                Log("Audio Station (Artist):                        Id=({0})", _artistId);
                Log("Audio Station (Album):                         Id=({0})", _albumId);
                Log("Audio Station (Track):                         Id=({0})", _trackId);
                Log("Audio Station (TrackGenreMap):                 Id=({0})", _trackGenreMapId);
                Log("Audio Station (TrackArtistMap):                Id=({0})", _trackArtistMapId);

                Log("Import Process Complete!");

                return true;
            }
            catch (Exception ex)
            {
                Log("Error finishing up import migration:  " + ex.Message, ex);
                return false;
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
