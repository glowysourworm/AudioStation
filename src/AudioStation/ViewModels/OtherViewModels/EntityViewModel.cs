using AudioStation.Core.Model;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.OtherViewModels
{
    public class EntityViewModel : ViewModelBase
    {
        int _id;
        LibraryEntryType _type;

        public int Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryEntryType Type
        {
            get { return _type; }
            private set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public EntityViewModel(int id, LibraryEntryType type)
        {
            this.Id = id;
            this.Type = type;
        }
    }
}
