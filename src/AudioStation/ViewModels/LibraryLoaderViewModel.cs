using System.Collections.ObjectModel;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Utility;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.ViewModels.LibraryLoaderViewModels;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels
{
    public class LibraryLoaderViewModel : PrimaryViewModelBase
    {
        #region Backing Fields (private)
        LibraryLoaderCDImportViewModel _importCDViewModel;
        LibraryLoaderImportViewModel _importViewModel;
        LibraryLoaderImportRadioViewModel _importRadioBasicViewModel;
        LibraryLoaderDownloadMusicBrainzViewModel _downloadMusicBrainzViewModel;


        KeyedObservableCollection<int, LibraryWorkItemViewModel> _libraryWorkItems;

        ObservableCollection<LibraryWorkItemViewModel> _libraryWorkItemsSelected;

        LibraryLoadType _selectedLibraryNewWorkItemType;
        #endregion

        #region Properties (public)
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
        public LibraryLoaderCDImportViewModel ImportCDViewModel
        {
            get { return _importCDViewModel; }
            set { this.RaiseAndSetIfChanged(ref _importCDViewModel, value); }
        }
        #endregion

        public LibraryLoaderViewModel(IConfigurationManager configurationManager,
                                      IIocEventAggregator eventAggregator,

                                      // View Models
                                      LibraryLoaderCDImportViewModel importCDViewModel,
                                      LibraryLoaderImportViewModel importViewModel,
                                      LibraryLoaderImportRadioViewModel importRadioBasicViewModel,
                                      LibraryLoaderDownloadMusicBrainzViewModel downloadMusicBrainzViewModel)
        {
            // Filtering of the library loader work items
            this.LibraryWorkItems = new KeyedObservableCollection<int, LibraryWorkItemViewModel>(x => x.Id);

            this.LibraryWorkItemsSelected = this.LibraryWorkItems;

            this.ImportCDViewModel = importCDViewModel;
            this.ImportViewModel = importViewModel;
            this.ImportRadioBasicViewModel = importRadioBasicViewModel;
            this.DownloadMusicBrainzViewModel = downloadMusicBrainzViewModel;

            eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(viewModel =>
            {
                if (this.LibraryWorkItems.ContainsKey(viewModel.Id))
                    ApplicationHelpers.MapOnto(viewModel, this.LibraryWorkItems[viewModel.Id]);
                else
                    this.LibraryWorkItems.Add(viewModel);
            });


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

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {

        }
    }
}
