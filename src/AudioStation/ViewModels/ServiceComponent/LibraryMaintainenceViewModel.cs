using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Worker;

namespace AudioStation.ViewModels.ServiceComponent
{
    public class LibraryMaintainenceViewModel : ServiceComponentViewModelBase
    {
        ServiceComponentPartViewModelBase _selectedWorker;

        public ServiceComponentPartViewModelBase SelectedWorker
        {
            get { return _selectedWorker; }
            set { this.RaiseAndSetIfChanged(ref _selectedWorker, value); }
        }

        public LibraryMaintainenceViewModel() : base("Library Maintainence")
        {
            this.AddComponentPart(new LibraryLoaderFileCheckerViewModel(true));

            // Initial Worker
            this.SelectedWorker = this.ComponentParts.First();
        }

        public override bool CanExecute()
        {
            return this.Loaded && !this.Loading;
        }
        public override bool CanLoad()
        {
            return !this.Loaded && !this.Loading;
        }
        public override bool CanReset()
        {
            return this.Loaded && !this.Loading;
        }

        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(part => part.Id == componentPartId)
                    .Load(configuration, audioStationController, progressHandler);
            }
        }
        public override void Execute(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(part => part.Id == componentPartId)
                    .Execute(progressHandler);
            }
        }
        public override void Reset(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(part => part.Id == componentPartId)
                    .Reset(progressHandler);
            }
        }
    }
}
