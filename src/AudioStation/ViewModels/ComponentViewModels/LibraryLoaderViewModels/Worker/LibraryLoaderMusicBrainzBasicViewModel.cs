using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzBasicViewModel : LibraryLoaderWorkerViewModelBase
    {
        public LibraryLoaderMusicBrainzBasicViewModel()
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID", -1, false)
        {
        }
        public LibraryLoaderMusicBrainzBasicViewModel(int workflowId)
            : base("Music Brainz (basic)", "Downloads basic tag details for recordings in the library with a Music Brainz ID", workflowId, true)
        {
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            base.Load(configuration, audioStationController, progressHandler);

            try
            {
                var results = audioStationController.ServiceController
                                                    .GetDataService<IAudioStationDbClient>()
                                                    .GetEntities<AcoustIDLookupResult>();
                var counter = 0;

                foreach (var result in results.GroupBy(x => x.MusicBrainzRecordingId))
                {
                    progressHandler(results.Count(), counter++, 0, "Loading: Music Brainz Id=" + result.Key);

                    this.WorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        HasErrors = false,
                        InProgress = false,
                        IsCompleted = false,
                        Load = new LibraryLoaderLoadViewModel()
                        {
                            DisplayText = result.First().FileName,
                            Data = new LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>()
                            {
                                EntitySet = new ObservableCollection<AcoustIDLookupResult>(result)
                            }
                        },
                        LoadType = LibraryLoadType.MusicBrainzBasic,
                        Output = new LibraryLoaderOutputViewModel()
                        {
                            Output = new LibraryLoaderEntitySetOutputViewModel<TagSmall>()
                        },
                        Progress = 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }

            this.Loaded = true;
        }
    }
}
