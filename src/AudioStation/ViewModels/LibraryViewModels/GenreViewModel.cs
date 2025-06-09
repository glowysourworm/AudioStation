using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class GenreViewModel : ViewModelBase
    {
        int _id;
        string _name;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public GenreViewModel()
        {
            this.Name = string.Empty;
        }
    }
}
