using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzBasicViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        public LibraryLoaderMusicBrainzBasicViewModel()
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID")
        {
        }

        public LibraryLoaderMusicBrainzBasicViewModel(LibraryImporterConfigurationViewModel configuration)
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID")
        {
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

            return new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(this.Id, LibraryLoadType.MusicBrainzBasic, new IAcoustIDLookupResult[] { loadItem.SelectedAcoustIDResult });
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var result = new List<ILibraryLoaderLoad>();
                var counter = 0;

                foreach (var entity in loadItems)
                {
                    // No AcoustID Result!
                    if (entity.SelectedAcoustIDResult == null)
                        continue;

                    progressHandler(loadItems.Count(), counter++, 0, 0, "Loading: Music Brainz Id=" + entity.SelectedAcoustIDResult.MusicBrainzRecordingId);

                    result.Add(new LibraryLoaderLoad<IEnumerable<IAcoustIDLookupResult>>(this.Id, LibraryLoadType.MusicBrainzBasic, new IAcoustIDLookupResult[] { entity.SelectedAcoustIDResult }));
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
