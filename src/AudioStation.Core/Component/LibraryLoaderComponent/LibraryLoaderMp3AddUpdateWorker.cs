using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderMp3AddUpdateWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IOutputController _outputController;

        public LibraryLoaderMp3AddUpdateWorker(IModelController modelController,
                                               IOutputController outputController) : base(outputController)
        {
            _modelController = modelController;
            _outputController = outputController;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var fileLoadError = false;
            var fileAvailable = false;
            var fileErrorMessasge = "";
            var generalError = false;

            var fileLoad = workItem.GetWorkItem() as LibraryLoaderFileLoad;

            // Processing...
            workItem.Start();

            foreach (var file in fileLoad.GetPendingFiles())
            {
                var entry = LoadLibraryEntry(workItem.GetId(), file, out fileErrorMessasge, out fileAvailable, out fileLoadError);

                // Set Work Item
                if (entry == null)
                {
                    RaiseLog(workItem.GetId(), "Mp3 load failed:  {0}", LogLevel.Error, file);

                    generalError = true;
                }
                else
                {
                    // Add to database (thread-safe call to create DbContext)
                    _modelController.AddUpdateLibraryEntry(file, fileAvailable, fileLoadError, fileErrorMessasge, entry);

                    RaiseLog(workItem.GetId(), "Mp3 load success:  {0}", LogLevel.Information, file);
                }

                fileLoad.SetComplete(file, entry != null);

                // Report -> UI Dispatcher (progress update)
                //
                Report(new LibraryWorkItem()
                {
                    Id = workItem.GetId(),
                    HasErrors = workItem.GetHasErrors(),
                    LoadState = workItem.GetLoadState(),
                    LoadType = workItem.GetLoadType(),
                    Runtime = DateTime.Now.Subtract(workItem.GetStartTime()),
                    PercentComplete = workItem.GetPercentComplete(),
                    LastMessage = file
                });
            }

            if (generalError)
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public TagLib.File LoadLibraryEntry(int workItemId, string file, out string fileErrorMessage, out bool fileAvailable, out bool fileLoadError)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            // File load parameters
            fileAvailable = Path.Exists(file);
            fileErrorMessage = "";
            fileLoadError = false;

            TagLib.File fileRef = null;

            try
            {
                fileRef = TagLib.File.Create(file);

                if (fileRef == null)
                {
                    fileErrorMessage = "Unable to load tag from file";
                    fileLoadError = true;
                }

                return fileRef;
            }
            catch (Exception ex)
            {
                fileErrorMessage = ex.Message;
                fileLoadError = true;

                RaiseLog(workItemId, "Mp3 file load error:  {0}", LogLevel.Error, ex.Message);
            }

            return null;
        }
    }
}
