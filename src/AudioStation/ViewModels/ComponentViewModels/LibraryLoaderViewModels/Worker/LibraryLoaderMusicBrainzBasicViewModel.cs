using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzBasicViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioStationDbClient _audioStationDbClient;

        public LibraryLoaderMusicBrainzBasicViewModel(
                IIocEventAggregator eventAggregator,
                ILibraryLoaderWorkerService libraryLoaderService,
                IAudioStationDbClient audioStationDbClient)
            : base(eventAggregator, libraryLoaderService)
        {
            _audioStationDbClient = audioStationDbClient;
        }

        protected override void InitializeWork(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var results = _audioStationDbClient.GetEntities<AcoustIDLookupResult>();

                foreach (var result in results.GroupBy(x => x.MusicBrainzRecordingId))
                {
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
        }
    }
}
