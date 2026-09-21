using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileCheckerViewModel : LibraryLoaderWorkerViewModelBase<FileReference>
    {
        public LibraryLoaderFileCheckerViewModel()
            : base("File Checker", "Verifies integrity of files related to Audio Station's library")
        {

        }

        protected override ILibraryLoaderLoad CreateWorkLoad(FileReference loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var result = CreateWorkLoads(new FileReference[] { loadItem }, configuration, audioStationController, progressHandler);

            return result.FirstOrDefault();
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<FileReference> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var result = new List<ILibraryLoaderLoad>();
                var counter = 0;

                foreach (var entity in loadItems)
                {
                    progressHandler(1, 1, loadItems.Count(), counter++, "Loading:  " + entity.FileName);

                    var workLoad = new LibraryLoaderLoad<FileReference>(this.Id, LibraryLoadType.FileChecker, entity.FileName, entity);

                    result.Add(workLoad);
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
        protected override void CompleteWorkItem(LibraryWorkItemViewModel workItem)
        {
            throw new NotImplementedException();
        }
    }
}
