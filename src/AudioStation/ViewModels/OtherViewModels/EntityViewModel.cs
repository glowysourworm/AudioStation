using AudioStation.Core.Model;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.OtherViewModels
{
    public class EntityViewModel : ViewModelBase
    {
        int _id;
        LibraryEntityType _type;

        public int Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryEntityType Type
        {
            get { return _type; }
            private set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public EntityViewModel(int id, LibraryEntityType type)
        {
            this.Id = id;
            this.Type = type;
        }
    }
}
