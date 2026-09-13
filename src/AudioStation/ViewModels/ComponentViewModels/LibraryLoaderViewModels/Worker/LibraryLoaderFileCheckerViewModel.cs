using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileCheckerViewModel : LibraryLoaderWorkerViewModelBase
    {
        public LibraryLoaderFileCheckerViewModel()
            : base("File Checker", "Verifies integrity of files related to Audio Station's library", -1, false)
        {

        }
        public LibraryLoaderFileCheckerViewModel(int workflowId)
            : base("File Checker", "Verifies integrity of files related to Audio Station's library", workflowId, true)
        {

        }
        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            base.Load(configuration, audioStationController, progressHandler);

            try
            {
                var results = audioStationController.ServiceController
                                                    .GetDataService<IAudioStationDbClient>()
                                                    .GetEntities<FileReference>();
                var counter = 0;



                // TODO: Create an entity set load with progress updater (for several hundred at once)
                foreach (var result in results)
                {
                    progressHandler(results.Count(), counter++, 0, "Loading:  " + result.FileName);

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

                this.Loaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }
    }
}
