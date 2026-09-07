using System.Collections.ObjectModel;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public class LibraryLoaderViewModel : ComponentViewModelBase
    {
        ObservableCollection<LibraryLoaderWorkerViewModelBase> _loaderTasks;

        public ObservableCollection<LibraryLoaderWorkerViewModelBase> LoaderTasks
        {
            get { return _loaderTasks; }
            set { this.RaiseAndSetIfChanged(ref _loaderTasks, value); }
        }

        public LibraryLoaderViewModel() : base("Library Loader")
        {
            this.LoaderTasks = new ObservableCollection<LibraryLoaderWorkerViewModelBase>();
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController audioStationViewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.LoaderTasks.Add(audioStationViewModelController.GetComponent<LibraryLoaderAcoustIDViewModel>());
            this.LoaderTasks.Add(audioStationViewModelController.GetComponent<LibraryLoaderFileCheckerViewModel>());
            this.LoaderTasks.Add(audioStationViewModelController.GetComponent<LibraryLoaderFileConverterViewModel>());
            this.LoaderTasks.Add(audioStationViewModelController.GetComponent<LibraryLoaderMusicBrainzBasicViewModel>());
            this.LoaderTasks.Add(audioStationViewModelController.GetComponent<LibraryLoaderMusicBrainzAlbumArtViewModel>());

            foreach (var task in this.LoaderTasks)
                task.Initialize(configuration, audioStationViewModelController, progressHandler);
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader componentViewModelLoader, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            foreach (var task in this.LoaderTasks)
                task.Load(configuration, componentViewModelLoader, progressHandler);
        }
    }
}
