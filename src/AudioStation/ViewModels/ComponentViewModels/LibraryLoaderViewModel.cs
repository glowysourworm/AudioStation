using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public class LibraryLoaderViewModel : ComponentViewModelBase
    {
        private readonly IAudioConverter _audioConverter;
        private readonly IIocEventAggregator _eventAggregator;

        ObservableCollection<LibraryLoaderWorkerViewModelBase> _loaderTasks;

        public ObservableCollection<LibraryLoaderWorkerViewModelBase> LoaderTasks
        {
            get { return _loaderTasks; }
            set { this.RaiseAndSetIfChanged(ref _loaderTasks, value); }
        }

        public LibraryLoaderViewModel(IIocEventAggregator eventAggregator, IAudioConverter audioConverter) : base("Library Loader")
        {
            _audioConverter = audioConverter;
            _eventAggregator = eventAggregator;

            this.LoaderTasks = new ObservableCollection<LibraryLoaderWorkerViewModelBase>();
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.LoaderTasks.Add(new LibraryLoaderAcoustIDViewModel(_eventAggregator, libraryLoaderWorkerService));
            this.LoaderTasks.Add(new LibraryLoaderFileCheckerViewModel(_eventAggregator, libraryLoaderWorkerService, audioStationDbClient));
            this.LoaderTasks.Add(new LibraryLoaderFileConverterViewModel(_audioConverter, _eventAggregator, libraryLoaderWorkerService));
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzBasicViewModel(_eventAggregator, libraryLoaderWorkerService, audioStationDbClient));
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzAlbumArtViewModel(_eventAggregator, libraryLoaderWorkerService, audioStationDbClient));

            foreach (var task in this.LoaderTasks)
                task.Initialize(configuration, audioStationController, progressHandler);
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            foreach (var task in this.LoaderTasks)
                task.Load(configuration, audioStationController, progressHandler);
        }
    }
}
