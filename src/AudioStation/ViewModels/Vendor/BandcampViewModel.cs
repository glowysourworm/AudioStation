using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
using AudioStation.Event;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.Vendor
{
    public class BandcampViewModel : PrimaryViewModelBase
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

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
        }
    }
}
