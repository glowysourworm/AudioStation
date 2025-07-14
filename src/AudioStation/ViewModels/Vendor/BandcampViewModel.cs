using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
using AudioStation.Event;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.Vendor
{
    [IocExportDefault]
    public class BandcampViewModel : ViewModelBase
    {
        SimpleCommand<string> _searchBandcampCommand;

        public SimpleCommand<string> SearchBandcampCommand
        {
            get { return _searchBandcampCommand; }
            set { RaiseAndSetIfChanged(ref _searchBandcampCommand, value); }
        }

        [IocImportingConstructor]
        public BandcampViewModel(IBandcampClient bandcampClient, IIocEventAggregator eventAggregator)
        {
            this.SearchBandcampCommand = new SimpleCommand<string>(async (endpoint) =>
            {
                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Calling Bandcamp API"));

                await bandcampClient.Download(endpoint);

                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            });
        }
    }
}
