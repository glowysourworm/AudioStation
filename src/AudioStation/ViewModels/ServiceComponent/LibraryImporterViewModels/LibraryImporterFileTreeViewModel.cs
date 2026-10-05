namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    public class LibraryImporterFileTreeViewModel : FileTreeViewModel
    {
        public LibraryImporterFileTreeViewModel()
        {
        }

        public void Remove(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            var nodes = this.RecursiveWhere(predicate);

            BeginUpdate();

            // Have to check each time because of branches that might have been 
            // related
            //
            foreach (var node in nodes)
            {
                if (Contains(node))
                    Remove(node);
            }

            EndUpdate();
        }

        public bool RecursiveAny(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveAny<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public IEnumerable<LibraryImporterFileTreeNodeViewModel> RecursiveWhere(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveWhere(predicate);
        }
        public LibraryImporterFileTreeNodeViewModel? RecursiveFirst(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveFirst<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public int RecursiveCount(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveCount<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public void RecursiveForEach(Action<LibraryImporterFileTreeNodeViewModel> action)
        {
            base.RecursiveForEach<LibraryImporterFileTreeNodeViewModel>(action);
        }
    }
}