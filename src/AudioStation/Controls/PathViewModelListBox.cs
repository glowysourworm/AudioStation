using System.Windows.Controls;

using SimpleWpf.ViewModel;

namespace AudioStation.Controls
{
    public class PathViewModelListBox : ListBox
    {
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            var viewModel = this.DataContext as RecursiveDispatcherViewModel<PathViewModel>;

            if (viewModel != null)
            {
                viewModel.NodeValue.IsSelected = this.SelectedItems.Contains(viewModel);

                viewModel.RecurseForEach(node =>
                {
                    node.NodeValue.IsSelected = this.SelectedItems.Contains(node);
                });
            }
        }
    }
}
