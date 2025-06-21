using AudioStation.Core.Model;

using SimpleWpf.Extensions;

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

        public GenreViewModel(int id) : base(id, LibraryEntityType.Genre)
        {
            this.Name = string.Empty;
        }
    }
}
