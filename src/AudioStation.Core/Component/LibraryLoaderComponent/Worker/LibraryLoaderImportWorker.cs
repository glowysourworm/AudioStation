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

        private const int WORK_STEPS = 6;

        private string _destinationPath;

        public LibraryLoaderImportWorker(LibraryLoaderWorkItem workItem,
                                         IAudioStationDbClient audioStationDbClient,
                                         IFileController fileController)
            : base(workItem)
        {
            _fileController = fileController;
            _audioStationDbClient = audioStationDbClient;

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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to copy (source) -> (destination):  " + ex.Message, ex);
                return false;
            }
        }

        private bool EmbedTagData()
        {
            return true;
        }

        private bool ImportDatabaseRecords()
        {
            return true;
        }

        private bool FinishUpMigration()
        {
            return true;
        }
    }
}
