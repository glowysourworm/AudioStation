using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class EntityViewModel : ViewModelBase
    {
        Guid _id;
        LibraryEntryType _type;

        public Guid Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryEntryType Type
        {
            get { return _type; }
            private set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public EntityViewModel(Guid id, LibraryEntryType type)
        {
            this.Id = id;
            this.Type = type;
        }
    }
}
