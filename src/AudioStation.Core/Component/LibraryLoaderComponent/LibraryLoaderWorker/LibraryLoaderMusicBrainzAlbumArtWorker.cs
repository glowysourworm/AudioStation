using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderMusicBrainzAlbumArtWorker : LibraryLoaderWorker<LibraryLoaderObjectLoad<Guid>, LibraryLoaderTagOutput>
    {
        private readonly IMusicBrainzClient _musicBrainzClient;

        int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzAlbumArtWorker(IMusicBrainzClient musicBrainzClient, LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _musicBrainzClient = musicBrainzClient;
        }

        public override int GetNumberOfWorkSteps()
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
                Log("Music Brainz front cover lookup started:  " + this.Load.Load);

                var frontCover = _musicBrainzClient.GetFrontArt(new AudioStationTagServiceModel(this.Load.Load));

                if (frontCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + this.Load.Load);

                    // ATL Tag -> Front Cover
                    this.Output.Tag.EmbeddedPictures.Add(frontCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + this.Load.Load);
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
                Log("Music Brainz back cover lookup started:  " + this.Load.Load);

                var backCover = _musicBrainzClient.GetBackArt(new AudioStationTagServiceModel(this.Load.Load));

                if (backCover != null)
                {
                    Log("Music Brainz client lookup finished:  " + this.Load.Load);

                    // ATL Tag -> Front Cover
                    this.Output.Tag.EmbeddedPictures.Add(backCover);
                }

                else
                {
                    Log("Music Brainz client lookup error:  " + this.Load.Load);
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
