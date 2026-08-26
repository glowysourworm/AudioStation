using System.Collections.ObjectModel;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

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

        public int RunLoaderTaskAsync(LibraryWorkItemViewModel workItem)
        {
            switch (workItem.LoadType)
            {
                case LibraryLoadType.Import:
                {
                    var workLoad = workItem.Load as LibraryLoaderImportLoadViewModel;

                    if (workLoad == null)
                        throw new ArgumentException("Invalid work load for Library Loader Import");

                    return _libraryLoader.RunLoaderTaskAsync(new LibraryLoaderParameters<LibraryLoaderImportLoad>(LibraryLoadType.Import,
                        new LibraryLoaderImportLoad(workLoad.SourceFolder,
                                                    workLoad.DestinationFolder,
                                                    workLoad.SourceFile,
                                                    workLoad.GroupingType,
                                                    workLoad.NamingType,
                                                    workLoad.IncludeMusicBrainzDetail,
                                                    workLoad.IdentifyUsingAcoustID,
                                                    workLoad.ImportFileMigration,
                                                    workLoad.MigrationDeleteSourceFiles,
                                                    workLoad.MigrationDeleteSourceFolders,
                                                    workLoad.MigrationOverwriteDestinationFiles)));
                }
                case LibraryLoadType.AcoustID:
                {
                    var workLoad = workItem.Load as LibraryLoaderFileLoadViewModel;

                    if (workLoad == null)
                        throw new ArgumentException("Invalid work load for Library Loader AcoustID Lookup");

                    return _libraryLoader.RunLoaderTaskAsync(
                        new LibraryLoaderParameters<LibraryLoaderFileLoad>(LibraryLoadType.AcoustID, new LibraryLoaderFileLoad(workLoad.FullPath)));
                }
                case LibraryLoadType.MusicBrainzTagSmall:
                {
                    var workLoad = workItem.Load as LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>;

                    if (workLoad == null)
                        throw new ArgumentException("Invalid work load for Library Loader Music Brainz Import");

                    return _libraryLoader.RunLoaderTaskAsync(
                        new LibraryLoaderParameters<LibraryLoaderEntitySetLoad<AcoustIDLookupResult>>(LibraryLoadType.MusicBrainzTagSmall, new LibraryLoaderEntitySetLoad<AcoustIDLookupResult>(workLoad.EntitySet)));
                }
                case LibraryLoadType.ImportRadio:
                default:
                    throw new Exception("Unhandled Libary Loader load type");
            }
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
                })),
                HasErrors = !sender.ResultSteps.Any() ? false : sender.ResultSteps.Any(x => !x.Result),
                InProgress = true,
                Progress = !sender.ResultSteps.Any() ? 0 : (sender.ResultSteps.Count(x => x.Completed) / (double)sender.ResultSteps.Count())
            };

            _eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Publish(viewModel);
        }

        private void LibraryLoader_WorkItemComplete(LibraryLoaderWorkItem sender)
        {
            var viewModel = new LibraryWorkItemViewModel()
            {
                Id = sender.GetId(),
                LoadType = sender.GetLoadType(),
                LogMessages = new ObservableCollection<LogMessageViewModel>(sender.GetOutputItem().Log.Select(x => new LogMessageViewModel()
                {
                    Level = x.Level,
                    Message = x.Message,
                    Timestamp = x.Timestamp,
                    Type = x.Type
                })),
                IsCompleted = true,
                WorkSteps = new ObservableCollection<LibraryLoaderWorkStepViewModel>(sender.GetOutputItem().Results.Select(x => new LibraryLoaderWorkStepViewModel()
                {
                    Complete = x.Completed,
                    Message = x.Message,
                    StepNumber = x.StepNumber,
                    Success = x.Result
                })),
                HasErrors = !sender.GetOutputItem().Results.Any() ? false : sender.GetOutputItem().Results.Any(x => !x.Result),
                InProgress = false,
                Progress = ((sender.GetOutputItem().Results.Count(x => x.Completed) / (double)sender.GetOutputItem().Results.Count()))
            };

            _eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Publish(viewModel);
        }
    }
}
