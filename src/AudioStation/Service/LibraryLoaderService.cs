using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.LogViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Service
{
    [IocExport(typeof(ILibraryLoaderService))]
    public class LibraryLoaderService : ILibraryLoaderService
    {
        private readonly ILibraryLoader _libraryLoader;
        private readonly IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public LibraryLoaderService(ILibraryLoader libraryLoader,
                                    IIocEventAggregator eventAggregator)
        {
            _libraryLoader = libraryLoader;
            _eventAggregator = eventAggregator;

            libraryLoader.WorkItemComplete += LibraryLoader_WorkItemComplete;
            libraryLoader.WorkItemUpdate += LibraryLoader_WorkItemUpdate;
        }

        public void RunLoaderTaskAsync(LibraryLoaderImportLoad workLoad)
        {
            // Show Dialog
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Importing Audio File"));

            _libraryLoader.RunLoaderTaskAsync(new LibraryLoaderParameters<LibraryLoaderImportLoad>(LibraryLoadType.Import, workLoad));
        }

        public void RunLoaderTaskAsync(LibraryLoaderEntityLoad workLoad)
        {
            // Show Dialog
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Importing Audio File"));

            _libraryLoader.RunLoaderTaskAsync(new LibraryLoaderParameters<LibraryLoaderEntityLoad>(LibraryLoadType.DownloadMusicBrainz, workLoad));
        }

        private void LibraryLoader_WorkItemUpdate(LibraryLoaderWorkItemUpdate sender)
        {
            var viewModel = new LibraryWorkItemViewModel()
            {
                Id = sender.Id,
                LoadType = sender.Type,
                LogMessages = new ObservableCollection<LogMessageViewModel>(sender.Log.Select(x => new LogMessageViewModel()
                {
                    Level = x.Level,
                    Message = x.Message,
                    Timestamp = x.Timestamp,
                    Type = x.Type
                })),
                IsCompleted = sender.IsCompleted,
                WorkSteps = new ObservableCollection<LibraryLoaderWorkStepViewModel>(sender.ResultSteps.Select(x => new LibraryLoaderWorkStepViewModel()
                {
                    Complete = x.Completed,
                    Message = x.Message,
                    StepNumber = x.StepNumber,
                    Success = x.Result
                }))
            };

            _eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Publish(viewModel);
        }

        private void LibraryLoader_WorkItemComplete(LibraryLoaderOutputBase sender)
        {
            // Hide Dialog (if all tasks are complete)
            if (_libraryLoader.IsWorkCompleted())
            {
                var workOutput = sender as LibraryLoaderImportOutput;

                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
                _eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Publish(new LibraryLoaderImportOutputViewModel()
                {
                    AcoustIDResults = new ObservableCollection<LookupResultViewModel>(workOutput.AcoustIDResults
                                                                                                .SelectMany(x => x.Recordings)
                                                                                                .Zip(workOutput.AcoustIDResults)
                                                                                                .Select(pair => new LookupResultViewModel()
                    {
                        Id = new Guid(pair.Second.Id),
                        Score = pair.Second.Score,
                        MusicBrainzRecordingId = new Guid(pair.First.Id)
                    })),
                    AcoustIDSuccess = workOutput.AcoustIDSuccess,
                    FinalRecord = null,
                    ImportedRecord = workOutput.ImportedRecord.FileName,
                    ImportFileMoveSuccess = workOutput.Mp3FileMoveSuccess,
                    ImportFileName = workOutput.ImportedRecord.FileName,
                    ImportSuccess = workOutput.Mp3FileImportSuccess,
                    LogMessages = new ObservableCollection<string>(workOutput.Log.Select(x => x.Message)),
                    MusicBrainzCombinedRecordQuerySuccess = false,
                    MusicBrainzCombinedRecords = null,
                    MusicBrainzRecordingMatches = new ObservableCollection<MusicBrainzRecordingViewModel>(
                                                    workOutput.MusicBrainzRecordingMatches
                                                              .Select(ApplicationHelpers.Map<MusicBrainzRecording, MusicBrainzRecordingViewModel>)),

                    MusicBrainzRecordingMatchSuccess = workOutput.MusicBrainzRecordingMatchSuccess,
                    OutputFileName = workOutput.ImportedRecord.FileName,
                    TagEmbeddingSuccess = workOutput.TagEmbeddingSuccess
                });
            }
        }
    }
}
