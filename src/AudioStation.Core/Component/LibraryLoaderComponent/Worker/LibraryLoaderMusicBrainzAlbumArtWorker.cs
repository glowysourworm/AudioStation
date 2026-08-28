using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Payload;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility.FileUtility;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtWorker : LibraryLoaderWorker
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IFileController _fileController;

        const int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzAlbumArtWorker(
            IAudioStationDbClient audioStationDbClient,
            IMusicBrainzClient musicBrainzClient,
            IFileController fileController,
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

        protected override bool Work(int stepNumber, ref string message)
        {
            // Procedure: The GUID should be the Music Brainz IRecording.Id from the Vendor <-> TagSmall map
            //
            // 1) Get front artwork from Music Brainz
            // 2) Get back artwork from Music Brainz
            //

            switch (stepNumber)
            {
                case 1:
                    return WorkArtwork(FileTypes.FrontCover, ref message);
                case 2:
                    return WorkArtwork(FileTypes.BackCover, ref message);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkArtwork(FileTypes fileType, ref string message)
        {
            try
            {
                var vendorMap = this.Load.Get<LibraryLoaderEntityLoad<TagSmallVendorMap>>();
                Guid musicBrainzRecordingId = vendorMap.Entity.MusicBrainzRecordingId ?? Guid.Empty;

                if (vendorMap.Entity.MusicBrainzRecordingId == null)
                {
                    message = "Invalid or missing Music Brainz Recording Id";
                    return false;
                }

                Log("Music Brainz album art lookup started:  " + vendorMap.Entity.MusicBrainzRecordingId);

                AudioStationTagServiceResponse response = null;

                switch (fileType)
                {
                    case FileTypes.FrontCover:
                        response = _musicBrainzClient.ProcessRequest(new AudioStationTagServiceRequest(AudioStationTagRequestType.ArtworkFront, musicBrainzRecordingId));
                        break;
                    case FileTypes.BackCover:
                        response = _musicBrainzClient.ProcessRequest(new AudioStationTagServiceRequest(AudioStationTagRequestType.ArtworkBack, musicBrainzRecordingId));
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
                    return false;

                var pictureInfo = (response.Payload as ArtworkPayload).GetPayload();

                if (pictureInfo != null)
                {
                    Log("Music Brainz client lookup finished:  " + vendorMap.Entity.MusicBrainzRecordingId);

                    // -> Store to file
                    var filePath = _fileController.StoreImage(pictureInfo,
                                                              vendorMap.Entity.TagSmall.Genre,
                                                              vendorMap.Entity.TagSmall.AlbumArtist,
                                                              vendorMap.Entity.TagSmall.Album,
                                                              fileType,
                                                              IFileController.StorageType.DiskCache, true);

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
                            TagSmallId = vendorMap.Entity.TagSmallId,
                            FileReferenceId = fileReference.Id
                        };

                        _audioStationDbClient.AddEntity(tagSmallFileReferenceMap);
                    }

                    // -> Report FileReference to the front end
                    this.Output.Get<LibraryLoaderEntitySetOutput<FileReference>>().Add(fileReference);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + vendorMap.Entity.MusicBrainzRecordingId);
                    return false;
                }

                message = "Music Brainz Album Art lookup successful";

                return true;
            }
            catch (Exception ex)
            {
                message = "Music Brainz service error: " + ex.Message;
                return false;
            }
        }
    }
}
