using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels
{
    public abstract class DirectoryTreeViewModel<T> : RecursiveNodeViewModel<T> where T : PathViewModelUI
    {
        protected DirectoryTreeViewModel(T nodeValue, RecursiveNodeViewModel<T> parent = null) : base(nodeValue, parent)
        {
        }
    }
}
