using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderMp3AddUpdateWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;

        public LibraryLoaderMp3AddUpdateWorker(IModelController modelController,
                                               IOutputController outputController) : base(outputController)
        {
            _modelController = modelController;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var fileLoadError = false;
            var fileAvailable = false;
            var fileErrorMessasge = "";

            var entry = LoadLibraryEntry(workItem.FileName, out fileErrorMessasge, out fileAvailable, out fileLoadError);

            // Set Work Item
            if (entry == null || entry.PossiblyCorrupt)
            {
                workItem.Set(LibraryWorkItemState.CompleteError, entry?.CorruptionReasons?.Join(",", x => x) ?? "Unknown Error");
            }
            else
            {
                // Add to database (thread-safe call to create DbContext)
                _modelController.AddUpdateLibraryEntry(workItem.FileName, fileAvailable, fileLoadError, fileErrorMessasge, entry);

                workItem.Set(LibraryWorkItemState.CompleteSuccessful, "");
            }
        }

        public TagLib.File LoadLibraryEntry(string file, out string fileErrorMessage, out bool fileAvailable, out bool fileLoadError)
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

                RaiseLog("Music file loaded:  {0}", LogMessageType.General, LogLevel.Information, file);

                return fileRef;
            }
            catch (Exception ex)
            {
                fileErrorMessage = ex.Message;
                fileLoadError = true;

                RaiseLog("Music file load error:  {0}", LogMessageType.General, LogLevel.Error, file);
            }

            return null;
        }
    }
}
