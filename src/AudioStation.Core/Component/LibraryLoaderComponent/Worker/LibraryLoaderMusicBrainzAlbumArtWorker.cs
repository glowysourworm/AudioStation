using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Payload.Input;
using AudioStation.Core.Service.Payload.Output;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility.FileUtility;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtWorker : LibraryLoaderWorker<TagSmallVendorMap, FileReference>
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAudioStationFileService _fileController;

        const int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzAlbumArtWorker(
            IAudioStationDbClient audioStationDbClient,
            IMusicBrainzClient musicBrainzClient,
            IAudioStationFileService fileController,
            LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _audioStationDbClient = audioStationDbClient;
            _musicBrainzClient = musicBrainzClient;
            _fileController = fileController;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }
        public static int GetNumberSteps()
        {
            return WORK_STEPS;
        }

        protected override LibraryWorkerStepResult Work(int stepNumber)
        {
            // Procedure: The GUID should be the Music Brainz IRecording.Id from the Vendor <-> TagSmall map
            //
            // 1) Get front artwork from Music Brainz
            // 2) Get back artwork from Music Brainz
            //

            switch (stepNumber)
            {
                case 1:
                    return WorkArtwork(FileTypes.FrontCover, stepNumber);
                case 2:
                    return WorkArtwork(FileTypes.BackCover, stepNumber);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkArtwork(FileTypes fileType, int stepNumber)
        {
            try
            {
                var vendorMap = this.Load.Payload;
                Guid musicBrainzRecordingId = vendorMap.MusicBrainzRecordingId ?? Guid.Empty;

                if (vendorMap.MusicBrainzRecordingId == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Invalid or missing Music Brainz Recording Id",
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultLevel.DataError
                    };
                }

                Log("Music Brainz album art lookup started:  " + vendorMap.MusicBrainzRecordingId);

                AudioStationTagServiceResponse response = null;

                switch (fileType)
                {
                    case FileTypes.FrontCover:
                        response = _musicBrainzClient.ProcessRequest(new AudioStationTagServiceRequest(AudioStationTagRequestType.ArtworkFront, new MusicBrainzLookupPayload(MusicBrainzLookupRequestType.MusicBrainzRecordingId, musicBrainzRecordingId)));
                        break;
                    case FileTypes.BackCover:
                        response = _musicBrainzClient.ProcessRequest(new AudioStationTagServiceRequest(AudioStationTagRequestType.ArtworkBack, new MusicBrainzLookupPayload(MusicBrainzLookupRequestType.MusicBrainzRecordingId, musicBrainzRecordingId)));
                        break;
                    case FileTypes.AudioFile:
                    case FileTypes.FanArt:
                        throw new Exception("Invalid file type");
                    default:
                        throw new Exception("Unhandled file type");
                }

                // Music Brainz return message
                Log(response.Message);

                if (!response.Success)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = response.Message,
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultLevel.ServiceFailure
                    };
                }

                var pictureInfo = (response.Payload as ArtworkPayload).GetPayload();

                if (pictureInfo != null)
                {
                    Log("Music Brainz client lookup finished:  " + vendorMap.MusicBrainzRecordingId);

                    // -> Store to file
                    var filePath = _fileController.StoreImage(pictureInfo,
                                                              vendorMap.TagSmall.Genre,
                                                              vendorMap.TagSmall.AlbumArtist,
                                                              vendorMap.TagSmall.Album,
                                                              fileType,
                                                              IAudioStationFileService.StorageType.DiskCache, true);

                    Log("Artwork saved to file:  " + filePath);

                    Log("Storing file reference information to database");

                    // File Reference
                    var fileReference = _audioStationDbClient.FirstEntity<FileReference>(x => x.FileName == filePath);

                    // Update
                    if (fileReference != null)
                    {
                        fileReference.FileErrorMessage = null;
                        fileReference.FileCorruptMessage = null;
                        fileReference.IsFileLoadError = false;
                        fileReference.Created = System.IO.File.GetCreationTime(filePath).ToUniversalTime();
                        fileReference.LastModified = System.IO.File.GetLastWriteTime(filePath).ToUniversalTime();
                        fileReference.CRC32 = FileHelpers.CalculateCRC32(filePath);

                        _audioStationDbClient.UpdateEntity(fileReference);
                    }

                    // Add
                    else
                    {
                        fileReference = new FileReference()
                        {
                            CRC32 = FileHelpers.CalculateCRC32(filePath),
                            Created = System.IO.File.GetCreationTimeUtc(filePath).ToUniversalTime(),
                            FileName = filePath,
                            FileErrorMessage = null,
                            FileCorruptMessage = null,
                            IsFileAvailable = true,
                            IsFileCorrupt = false,
                            IsFileLoadError = false,
                            LastModified = System.IO.File.GetLastWriteTimeUtc(filePath).ToUniversalTime(),
                        };

                        _audioStationDbClient.AddEntity(fileReference);

                        // Get Updated FileReference (TODO) (THIS SHOULD BE RETURNED FROM THE ABOVE METHOD)
                        fileReference = _audioStationDbClient.FirstEntity<FileReference>(x => x.FileName == fileReference.FileName);

                        var tagSmallFileReferenceMap = new TagSmallFileReferenceMap()
                        {
                            TagSmallId = vendorMap.TagSmallId,
                            FileReferenceId = fileReference.Id
                        };

                        _audioStationDbClient.AddEntity(tagSmallFileReferenceMap);
                    }

                    // -> Report FileReference to the front end
                    this.Output.SetPayload(fileReference);
                }

                else
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Music Brainz client lookup error:  " + vendorMap.MusicBrainzRecordingId,
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultLevel.DataError
                    };
                }

                return LibraryWorkerStepResult.Success(stepNumber, "Music Brainz Album Art lookup successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error getting album art: " + ex.Message);
            }
        }
    }
}
