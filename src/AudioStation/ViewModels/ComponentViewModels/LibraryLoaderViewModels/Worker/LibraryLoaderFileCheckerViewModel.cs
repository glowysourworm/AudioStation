using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileCheckerViewModel : LibraryLoaderWorkerViewModelBase
    {
        public LibraryLoaderFileCheckerViewModel()
            : base("File Checker", "Verifies integrity of files related to Audio Station's library")
        {

        }

        protected override IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var entities = audioStationController.ServiceController
                                                     .GetDataService<IAudioStationDbClient>()
                                                     .GetEntities<FileReference>();

                var result = new List<LibraryLoaderLoad>();
                var counter = 0;

                foreach (var entity in entities)
                {
                    progressHandler(1, 1, entities.Count(), counter++, "Loading:  " + entity.FileName);

                    var workLoad = new LibraryLoaderEntityLoad<FileReference>(this.Id, LibraryLoadType.FileChecker, entity);

                    result.Add(new LibraryLoaderLoad(LibraryLoadType.FileChecker, workLoad));
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
