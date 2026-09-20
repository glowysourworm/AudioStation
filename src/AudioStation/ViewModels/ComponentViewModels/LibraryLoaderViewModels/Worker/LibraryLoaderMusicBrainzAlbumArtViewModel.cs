using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        public LibraryLoaderMusicBrainzAlbumArtViewModel()
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library")
        {
        }

        public LibraryLoaderMusicBrainzAlbumArtViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library")
        {
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();
            var vendorMap = audioStationDbClient.FirstEntity<TagSmallVendorMap>(x => x.TagSmallId == loadItem.SelectedMusicBrainzRecordingMatch.Id);

            if (vendorMap != null)
                return new LibraryLoaderLoad<TagSmallVendorMap>(this.Id, LibraryLoadType.MusicBrainzAlbumArt, loadItem.DisplayName, vendorMap);

            else
                throw new Exception("Invalid Music Brainz Album Art Input:  Missing valid TagSmallVendorMap");
        }
        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();
                var result = new List<ILibraryLoaderLoad>();
                var counter = 0;

                foreach (var map in loadItems.Where(x => x.SelectedMusicBrainzRecordingMatch != null))
                {
                    progressHandler(loadItems.Count(), counter++, 0, 0, "Loading: Music Brainz Id=" + map.MusicBrainzTrackIDTag);

                    var vendorMap = audioStationDbClient.FirstEntity<TagSmallVendorMap>(x => x.TagSmallId == map.SelectedMusicBrainzRecordingMatch.Id);

                    if (vendorMap != null)
                        result.Add(new LibraryLoaderLoad<TagSmallVendorMap>(this.Id, LibraryLoadType.MusicBrainzAlbumArt, map.DisplayName, vendorMap));

                    else
                        throw new Exception("Invalid Music Brainz Album Art Input:  Missing valid TagSmallVendorMap");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }
        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            throw new NotImplementedException();
        }
        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            throw new NotImplementedException();
        }
        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            throw new NotImplementedException();
        }
    }
}
