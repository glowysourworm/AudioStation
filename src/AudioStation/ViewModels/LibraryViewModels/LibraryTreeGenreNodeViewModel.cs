using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryTreeGenreNodeViewModel : TreeViewNodeModelBase
    {
        private GenreViewModel _genre;

        public override object Key
        {
            get { return _genre.Name; }
        }


        public LibraryTreeGenreNodeViewModel(int recursionDepth, GenreViewModel genre, TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            if (genre == null || string.IsNullOrWhiteSpace(genre.Name))
                throw new ArgumentException("Invalid Genre Node");

            _genre = genre;
        }
    }
}
