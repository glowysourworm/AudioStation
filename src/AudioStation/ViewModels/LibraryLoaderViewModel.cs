using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Controller;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.ViewModels.LibraryLoaderViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryLoaderViewModel : ViewModelBase, IDisposable
    {
        private readonly IOutputController _outputController;
        private readonly ILibraryLoader _libraryLoader;

        #region Backing Fields (private)
        LibraryLoaderImportViewModel _importViewModel;
        LibraryLoaderImportRadioViewModel _importRadioBasicViewModel;
        LibraryLoaderDownloadMusicBrainzViewModel _downloadMusicBrainzViewModel;


        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItems;

        ObservableCollection<LibraryWorkItemViewModel> _libraryWorkItemsSelected;

        LibraryLoadType _selectedLibraryNewWorkItemType;

        SimpleCommand<LibraryLoadType> _runWorkItemCommand;
        #endregion

        #region Properties (public)
        public PlayStopPause LibraryLoaderState
        {
            // These forward / receive state requests to/from the library loader
            get { return _libraryLoader.GetState(); }
            set { OnLibraryLoaderStateRequest(value); }
        }
        public LibraryLoadType SelectedLibraryNewWorkItemType
        {
            get { return _selectedLibraryNewWorkItemType; }
            set { this.RaiseAndSetIfChanged(ref _selectedLibraryNewWorkItemType, value); }
        }
        public KeyedObservableCollection<int, LibraryWorkItemViewModel> LibraryWorkItems
        {
            get { return _libraryWorkItems; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItems, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> LibraryWorkItemsSelected
        {
            get { return _libraryWorkItemsSelected; }
            set { this.RaiseAndSetIfChanged(ref _libraryWorkItemsSelected, value); }
        }
        public LibraryLoaderImportViewModel ImportViewModel
        {
            get { return _importViewModel; }
            set { this.RaiseAndSetIfChanged(ref _importViewModel, value); }
        }
        public LibraryLoaderImportRadioViewModel ImportRadioBasicViewModel
        {
            get { return _importRadioBasicViewModel; }
            set { this.RaiseAndSetIfChanged(ref _importRadioBasicViewModel, value); }
        }
        public LibraryLoaderDownloadMusicBrainzViewModel DownloadMusicBrainzViewModel
        {
            get { return _downloadMusicBrainzViewModel; }
            set { this.RaiseAndSetIfChanged(ref _downloadMusicBrainzViewModel, value); }
        }
        #endregion

        [IocImportingConstructor]
        public LibraryLoaderViewModel(IModelController modelController,
                                      ILibraryLoader libraryLoader,
                                      IConfigurationManager configurationManager, 
                                      
                                      // View Models
                                      LibraryLoaderImportViewModel importViewModel,
                                      LibraryLoaderImportRadioViewModel importRadioBasicViewModel,
                                      LibraryLoaderDownloadMusicBrainzViewModel downloadMusicBrainzViewModel)
        {
            _libraryLoader = libraryLoader;

            // Filtering of the library loader work items
            this.LibraryWorkItems = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);

            this.LibraryWorkItemsSelected = this.LibraryWorkItems;

            this.ImportViewModel = importViewModel;
            this.ImportRadioBasicViewModel = importRadioBasicViewModel;
            this.DownloadMusicBrainzViewModel = downloadMusicBrainzViewModel;

            this.LibraryLoaderState = libraryLoader.GetState();

            libraryLoader.WorkItemUpdate += OnWorkItemUpdate;
            libraryLoader.ProcessingUpdate += OnLibraryProcessingChanged;

            //this.RunWorkItemCommand = new SimpleCommand<LibraryLoadType>(loadType =>
            //{
            //    var configuration = configurationManager.GetConfiguration();

            //    switch (loadType)
            //    {
            //        case LibraryLoadType.LoadMp3FileData:
            //        {
            //            var fileLoad = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.mp3", SearchOption.AllDirectories);
            //            var inputLoad = new LibraryLoaderFileLoad(configuration.DirectoryBase, fileLoad);
            //            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderFileLoad>(loadType, inputLoad));
            //        }
            //        break;
            //        case LibraryLoadType.LoadM3UFileData:
            //        {
            //            var fileLoad = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.m3u", SearchOption.AllDirectories);
            //            var inputLoad = new LibraryLoaderFileLoad(configuration.DirectoryBase, fileLoad);
            //            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderFileLoad>(loadType, inputLoad));
            //        }
            //        break;
            //        case LibraryLoadType.FillMusicBrainzIds:
            //        {
            //            var entities = modelController.GetAudioStationEntities<Mp3FileReference>();
            //            var inputLoad = new LibraryLoaderEntityLoad(entities);
            //            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderEntityLoad>(loadType, inputLoad));
            //        }
            //        break;
            //        case LibraryLoadType.ImportStagedFiles:
            //        {
            //            var fileLoad = ApplicationHelpers.FastGetFiles(configuration.DownloadFolder, "*.mp3", SearchOption.AllDirectories);
            //            var inputLoad = new LibraryLoaderImportLoad(configuration.DirectoryBase, fileLoad);
            //            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderFileLoad>(loadType, inputLoad));
            //        }
            //        break;
            //        case LibraryLoadType.ImportRadioFiles:
            //        {
            //            var fileLoad = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.m3u", SearchOption.AllDirectories);
            //            var inputLoad = new LibraryLoaderFileLoad(configuration.DirectoryBase, fileLoad);
            //            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderFileLoad>(loadType, inputLoad));
            //        }
            //        break;
            //        default:
            //            throw new Exception("Unhandled LibraryLoadType:  LibraryLoaderViewModel.cs");
            //    }
            //});
        }

        #region ILibraryLoader Events (these are all on the Dispatcher)
        private void RefreshWorkItems(LibraryWorkItem newItem)
        {
            foreach (var workItem in _libraryLoader.GetIdleWorkItems().Union(new LibraryWorkItem[] { newItem }))
            {
                // May not be a new item (we're consolidating code)
                if (workItem == null)
                    continue;

                if (!this.LibraryWorkItems.ContainsKey(workItem.Id))
                {
                    this.LibraryWorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        Id = workItem.Id,
                        HasErrors = workItem.HasErrors,
                        Progress = workItem.PercentComplete,
                        EstimatedCompletionTime = workItem.EstimatedCompletionTime,
                        FailureCount = workItem.FailureCount,
                        SuccessCount = workItem.SuccessCount,
                        LoadType = workItem.LoadType,
                        LoadState = workItem.LoadState,
                        LastMessage = workItem.LastMessage,
                    });
                }
                else
                {
                    var viewModel = this.LibraryWorkItems[workItem.Id];

                    viewModel.Progress = workItem.PercentComplete;
                    viewModel.HasErrors = workItem.HasErrors;
                    viewModel.FailureCount = workItem.FailureCount;
                    viewModel.SuccessCount = workItem.SuccessCount;
                    viewModel.EstimatedCompletionTime = workItem.EstimatedCompletionTime;
                    viewModel.LoadState = workItem.LoadState;
                    viewModel.LoadType = workItem.LoadType;
                    viewModel.LastMessage = workItem.LastMessage;
                }
            }
        }
        private void OnWorkItemUpdate(LibraryWorkItem workItem)
        {
            RefreshWorkItems(workItem);
        }

        private void OnLibraryProcessingChanged()
        {
            RefreshWorkItems(null);

            // Notify listeners. The getter draws from the library loader
            OnPropertyChanged("LibraryLoaderState");
        }
        #endregion

        private void OnLibraryLoaderStateRequest(PlayStopPause state)
        {
            switch (state)
            {
                case PlayStopPause.Play:
                    _libraryLoader.Start();
                    break;
                case PlayStopPause.Pause:
                case PlayStopPause.Stop:
                    _libraryLoader.Stop();
                    break;
                default:
                    throw new Exception("Unhandled play / stop / pause state:  MainViewModel.cs");
            }
        }

        public void Dispose()
        {
            _libraryLoader?.Dispose();
        }
    }
}
