using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.OtherViewModels
{
    public class DirectoryTreeViewModel : RecursiveDispatcherViewModel<PathViewModel>
    {
        public DirectoryTreeViewModel(PathViewModel nodeValue, RecursiveDispatcherViewModel<PathViewModel> parent = null)
            : base(nodeValue, parent)
        {
        }

        protected override RecursiveDispatcherViewModel<PathViewModel> Construct(PathViewModel nodeValue)
        {
            return new DirectoryTreeViewModel(nodeValue, this);
        }

        public bool HasSelectedParent()
        {
            if (this.Parent != null)
            {
                if (this.Parent.NodeValue.IsSelected)
                    return true;

                else
                    return (this.Parent as DirectoryTreeViewModel).HasSelectedParent();
            }

            return false;
        }
    }
}
