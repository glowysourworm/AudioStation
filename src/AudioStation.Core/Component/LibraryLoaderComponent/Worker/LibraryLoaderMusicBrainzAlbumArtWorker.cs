using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
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
                var musicBrainzId = this.Load.Get<Guid>();

                Log("Music Brainz front cover lookup started:  " + musicBrainzId);

                var frontCover = _musicBrainzClient.GetFrontArt(new AudioStationTagServiceModel(musicBrainzId));

                if (frontCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + musicBrainzId);

                    // ATL Tag -> Front Cover
                    this.Output.Get<IAudioStationTag>().EmbeddedPictures.Add(frontCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + musicBrainzId);
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
                var musicBrainzId = this.Load.Get<Guid>();

                Log("Music Brainz back cover lookup started:  " + musicBrainzId);

                var backCover = _musicBrainzClient.GetBackArt(new AudioStationTagServiceModel(musicBrainzId));

                if (backCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + musicBrainzId);

                    // ATL Tag -> Front Cover
                    this.Output.Get<IAudioStationTag>().EmbeddedPictures.Add(backCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + musicBrainzId);
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
