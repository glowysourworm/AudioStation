using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    [IocExportDefault]
    public class LibraryLoaderFileCheckerViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioStationDbClient _audioStationDbClient;

        [IocImportingConstructor]
        public LibraryLoaderFileCheckerViewModel(
                IIocEventAggregator eventAggregator,
                ILibraryLoaderWorkerService libraryLoaderService,
                IAudioStationDbClient audioStationDbClient)
            : base(eventAggregator, libraryLoaderService)
        {
            _audioStationDbClient = audioStationDbClient;
        }

        protected override void InitializeWorkItemsRun(IAudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var results = _audioStationDbClient.GetEntities<FileReference>();

                // TODO: Create an entity set load with progress updater (for several hundred at once)
                foreach (var result in results)
                {
                    this.WorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        HasErrors = false,
                        InProgress = false,
                        IsCompleted = false,
                        Load = new LibraryLoaderLoadViewModel()
                        {
                            DisplayText = result.FileName,
                            Data = new LibraryLoaderEntityLoadViewModel<FileReference>()
                            {
                                Entity = result
                            }
                        },
                        LoadType = LibraryLoadType.FileChecker,
                        Output = new LibraryLoaderOutputViewModel()
                        {
                            Output = new NoViewModel()
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

        public override void Dispose()
        {


        }
    }
}
