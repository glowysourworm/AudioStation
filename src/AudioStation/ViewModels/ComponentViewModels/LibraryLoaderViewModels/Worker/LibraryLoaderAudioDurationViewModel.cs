using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

using Microsoft.Extensions.Logging;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderAudioDurationViewModel : LibraryLoaderWorkerViewModelBase<LibraryImporterFileViewModel>
    {
        Dictionary<string, LibraryImporterFileViewModel> _loadItemDict;

        public LibraryLoaderAudioDurationViewModel() : base("Audio Encoding Checker", "Checks audio file for encoding and duration information; and stores the result with your import data.")
        {
            _loadItemDict = new Dictionary<string, LibraryImporterFileViewModel>();
        }

        protected override ILibraryLoaderLoad CreateWorkLoad(LibraryImporterFileViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            return CreateWorkLoads(new LibraryImporterFileViewModel[] { loadItem }, configuration, audioStationController, progressHandler).First();
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<LibraryImporterFileViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var result = new List<ILibraryLoaderLoad>();
                var totalCount = loadItems.Count();
                var counter = 0;

                foreach (var stagedFile in loadItems)
                {
                    progressHandler(1, 1, totalCount, counter++, "Loading: " + stagedFile.DisplayName);

                    // Keep track of load items for post processing
                    _loadItemDict.Add(stagedFile.FullPath, stagedFile);

                    result.Add(new LibraryLoaderLoad<LibraryLoaderFilePayload>(this.Id, LibraryLoadType.AudioEncoding, stagedFile.FullPath,
                               new LibraryLoaderFilePayload(stagedFile.FullPath)));
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            return new LibraryLoaderLoadViewModel()
            {
                DisplayName = workLoad.DisplayName,
                LoadType = workLoad.LoadType,
                OwnerId = workLoad.OwnerId,
                Payload = workLoad.Payload
            };
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            return new LibraryLoaderOutputViewModel()
            {
                Payload = workOutput.Payload
            };
        }
        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            var input = workItem.Load.Payload as LibraryLoaderFilePayload;

            if (input == null)
                throw new Exception("Corrupt library loader work item");

            return new LibraryLoaderLoad<LibraryLoaderFilePayload>(this.Id, LibraryLoadType.AudioEncoding, input.File,
                   new LibraryLoaderFilePayload(input.File));
        }
        protected override void CompleteWorkItem(LibraryWorkItemViewModel workItem)
        {
            var input = workItem.Load.Payload as LibraryLoaderFilePayload;
            var output = workItem.Output.Payload as LibraryLoaderAudioEncodingOutputPayload;

            if (input == null || output == null)
                throw new Exception("Corrupt library loader work item");

            if (!_loadItemDict.ContainsKey(input.File))
                throw new Exception("Corrupt library loader work item");

            // Staged File
            var loadItem = _loadItemDict[input.File];

            // Tag (record) Duration
            loadItem.TagRecordDirty.DurationMilliseconds = (int)output.Duration.TotalMilliseconds;
        }
    }
}
