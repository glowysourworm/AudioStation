using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.LibraryLoaderViewModels.Interface;
using AudioStation.ViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ServiceComponent
{
    public class LibraryLoaderViewModel : ServiceComponentViewModelBase
    {
        private readonly IAudioConverter _audioConverter;
        private readonly IIocEventAggregator _eventAggregator;

        ObservableCollection<ILibraryLoaderWorkerViewModel> _loaderTasks;

        public ObservableCollection<ILibraryLoaderWorkerViewModel> LoaderTasks
        {
            get { return _loaderTasks; }
            set { this.RaiseAndSetIfChanged(ref _loaderTasks, value); }
        }

        public LibraryLoaderViewModel(IIocEventAggregator eventAggregator, IAudioConverter audioConverter) : base("Library Loader")
        {
            _audioConverter = audioConverter;
            _eventAggregator = eventAggregator;

            this.LoaderTasks = new ObservableCollection<ILibraryLoaderWorkerViewModel>();
        }

        public override bool CanExecute()
        {
            return this.LoaderTasks != null && this.LoaderTasks.All(x => x.CanExecute());
        }
        public override bool CanLoad()
        {
            return !this.Loaded;
        }
        public override bool CanReset()
        {
            return false;
        }
        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.LoaderTasks.Add(new LibraryLoaderAcoustIDViewModel());
            this.LoaderTasks.Add(new LibraryLoaderFileCheckerViewModel());
            //this.LoaderTasks.Add(new LibraryLoaderFileConverterViewModel());
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzBasicViewModel());
            this.LoaderTasks.Add(new LibraryLoaderMusicBrainzAlbumArtViewModel());
        }

        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            //foreach (var task in this.LoaderTasks)
            //    task.Load(configuration, audioStationController, progressHandler);
        }

        public override void Execute(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            throw new NotImplementedException();
        }

        public override void Reset(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            throw new NotImplementedException();
        }
    }
}
