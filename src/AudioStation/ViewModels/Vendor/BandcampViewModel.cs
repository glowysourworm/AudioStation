using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Event;
using AudioStation.EventHandler;
using AudioStation.ViewModels.ComponentViewModels;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.Vendor
{
    public class BandcampViewModel : ComponentViewModelBase
    {
        SimpleCommand<string> _searchBandcampCommand;

        public SimpleCommand<string> SearchBandcampCommand
        {
            get { return _searchBandcampCommand; }
            set { RaiseAndSetIfChanged(ref _searchBandcampCommand, value); }
        }

        public BandcampViewModel(IBandcampClient bandcampClient, IIocEventAggregator eventAggregator)
        {
            this.SearchBandcampCommand = new SimpleCommand<string>(async (endpoint) =>
            {
                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Calling Bandcamp API"));

                await bandcampClient.Download(endpoint);

                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            });
        }

        protected override void InitializeWork(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
    }
}
