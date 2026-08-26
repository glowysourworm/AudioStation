using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderTagOutputViewModel : LibraryLoaderOutputViewModelBase
    {
        IAudioStationTag _tag;

        public IAudioStationTag Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }
    }
}
