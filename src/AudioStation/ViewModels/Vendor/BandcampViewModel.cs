using AudioStation.Core.Component.Vendor.Bandcamp.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

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
        public BandcampViewModel(IBandcampClient bandcampClient)
        {
            this.SearchBandcampCommand = new SimpleCommand<string>((endpoint) =>
            {
                bandcampClient.Download(endpoint);
            });
        }
    }
}
