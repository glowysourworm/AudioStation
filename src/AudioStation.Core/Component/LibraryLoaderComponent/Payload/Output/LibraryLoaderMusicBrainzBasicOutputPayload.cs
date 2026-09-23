using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output
{
    public class LibraryLoaderMusicBrainzBasicOutputPayload
    {
        public List<TagSmall> AcoustIDResults { get; set; }
        public TagSmall? MusicBrainzResult { get; set; }

        public LibraryLoaderMusicBrainzBasicOutputPayload()
        {
            this.AcoustIDResults = new List<TagSmall>();
        }
    }
}
