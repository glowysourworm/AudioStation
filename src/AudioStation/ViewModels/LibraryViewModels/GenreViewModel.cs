using AudioStation.Core.Model;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class GenreViewModel : EntityViewModel
    {
        string _name;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public GenreViewModel(Guid id) : base(id, LibraryEntryType.Genre)
        {
            this.Name = string.Empty;
        }
    }
}
