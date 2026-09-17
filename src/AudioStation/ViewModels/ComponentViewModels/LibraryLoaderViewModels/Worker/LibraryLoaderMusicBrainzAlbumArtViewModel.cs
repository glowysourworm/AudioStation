using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtViewModel : LibraryLoaderWorkerViewModelBase
    {
        public LibraryLoaderMusicBrainzAlbumArtViewModel()
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library")
        {
        }

        public LibraryLoaderMusicBrainzAlbumArtViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library")
        {
        }

        protected override IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var tagMaps = audioStationController.ServiceController
                                                    .GetDataService<IAudioStationDbClient>()
                                                    .GetEntities<TagSmallVendorMap>();

                var result = new List<LibraryLoaderLoad>();
                var counter = 0;

                foreach (var map in tagMaps.Where(x => x.MusicBrainzRecordingId != null))
                {
                    progressHandler(tagMaps.Count(), counter++, 0, 0, "Loading: Music Brainz Id=" + map.MusicBrainzRecordingId);

                    result.Add(new LibraryLoaderLoad(LibraryLoadType.MusicBrainzAlbumArt,
                               new LibraryLoaderEntityLoad<TagSmallVendorMap>(this.Id, LibraryLoadType.MusicBrainzAlbumArt, map)));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(LibraryLoaderLoad workLoad)
        {
            throw new NotImplementedException();
        }
        protected override LibraryLoaderOutputViewModel MapWorkOutput(LibraryLoaderOutput workOutput)
        {
            throw new NotImplementedException();
        }
        protected override LibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            throw new NotImplementedException();
        }
    }
}
