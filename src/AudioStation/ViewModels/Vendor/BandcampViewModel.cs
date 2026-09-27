using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Event;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.Vendor
{
    public class BandcampViewModel : ServiceComponentViewModelBase
    {
        SimpleCommand<string> _searchBandcampCommand;

        public SimpleCommand<string> SearchBandcampCommand
        {
            get { return _searchBandcampCommand; }
            set { RaiseAndSetIfChanged(ref _searchBandcampCommand, value); }
        }

        public BandcampViewModel(IBandcampClient bandcampClient, IIocEventAggregator eventAggregator) : base("Bandcamp")
        {
            this.SearchBandcampCommand = new SimpleCommand<string>(async (endpoint) =>
            {
                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Calling Bandcamp API"));

                await bandcampClient.Download(endpoint);

                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            });
        }
        public override bool CanExecute()
        {
            return true;
        }
        public override bool CanLoad()
        {
            return true;
        }
        public override bool CanReset()
        {
            return true;
        }

        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
        }
        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
        }
        public override void Execute(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
        }
        public override void Reset(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
        }
    }
}
