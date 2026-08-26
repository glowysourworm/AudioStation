using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtWorker : LibraryLoaderWorker
    {
        private readonly IMusicBrainzClient _musicBrainzClient;

        const int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzAlbumArtWorker(IMusicBrainzClient musicBrainzClient, LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _musicBrainzClient = musicBrainzClient;
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
                    return WorkFrontCover(ref message);
                case 2:
                    return WorkBackCover(ref message);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkFrontCover(ref string message)
        {
            try
            {
                var vendorMap = this.Load.Get<LibraryLoaderEntityLoad<TagSmallVendorMap>>();

                Log("Music Brainz front cover lookup started:  " + vendorMap.Entity.MusicBrainzRecordingId);

                var frontCover = _musicBrainzClient.GetFrontArt(new AudioStationTagServiceModel(vendorMap.Entity.MusicBrainzRecordingId));

                if (frontCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + vendorMap.Entity.MusicBrainzRecordingId);

                    // -> Store to file


                    Log("Saving artwork to file:  " + vendorMap.Entity.MusicBrainzRecordingId);

                    // -> Report FileReference to the front end
                    //this.Output.Get<LibraryLoaderEntitySetOutput<FileReference>>().EmbeddedPictures.Add(frontCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + vendorMap.Entity.MusicBrainzRecordingId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "Music Brainz service error: " + ex.Message;
                return false;
            }
        }

        private bool WorkBackCover(ref string message)
        {
            try
            {
                var vendorMap = this.Load.Get<LibraryLoaderEntityLoad<TagSmallVendorMap>>();

                Log("Music Brainz back cover lookup started:  " + vendorMap.Entity.MusicBrainzRecordingId);

                var backCover = _musicBrainzClient.GetBackArt(new AudioStationTagServiceModel(vendorMap.Entity.MusicBrainzRecordingId));

                if (backCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + vendorMap.Entity.MusicBrainzRecordingId);

                    // ATL Tag -> Front Cover
                    //this.Output.Get<IAudioStationTag>().EmbeddedPictures.Add(backCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + vendorMap.Entity.MusicBrainzRecordingId);
                    return false;
                }

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
