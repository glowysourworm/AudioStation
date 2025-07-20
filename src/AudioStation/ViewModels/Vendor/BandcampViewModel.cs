using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
using AudioStation.Event;
using AudioStation.EventHandler;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

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

        public override void Initialize(DialogProgressHandler progressHandler)
        {
        }

        public override void Dispose()
        {
        }
    }
}
