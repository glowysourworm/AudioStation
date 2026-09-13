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

        public override bool CanExecute()
        {
            return this.LoaderTasks != null && this.LoaderTasks.All(x => x.CanExecute());
        }

        protected override void InitializeWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.LoaderTasks.Add(new LibraryLoaderAcoustIDViewModel());
            this.LoaderTasks.Add(new LibraryLoaderFileCheckerViewModel());
            this.LoaderTasks.Add(new LibraryLoaderFileConverterViewModel());
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzBasicViewModel());
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzAlbumArtViewModel());
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            foreach (var task in this.LoaderTasks)
                task.Load(configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
    }
}
