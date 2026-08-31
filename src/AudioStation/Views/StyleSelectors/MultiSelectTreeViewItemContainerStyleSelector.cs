using System.Windows;
using System.Windows.Controls;

using SimpleWpf.ViewModel;

using Xceed.Wpf.Toolkit.Core.Utilities;

namespace AudioStation.Views.StyleSelectors
{
    public class MultiSelectTreeViewItemContainerStyleSelector : StyleSelector
    {
        public MultiSelectTreeViewItemContainerStyleSelector()
        {
        }
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var listBoxItem = VisualTreeHelperEx.FindAncestorByType<ListBoxItem>(container);

            if (listBoxItem == null)
                throw new NullReferenceException("Improper handling of MultiSelectTreeViewItemContainerStyleSelector");

            // SPECIFIC VIEW MODEL
            //
            var viewModel = listBoxItem.DataContext as RecursiveDispatcherViewModel<PathViewModel>;

            if (viewModel == null)
                throw new NullReferenceException("Improper handling of MultiSelectTreeViewItemContainerStyleSelector");

            // Leaf Nodes (File)
            if (!viewModel.NodeValue.IsDirectory)
                return listBoxItem.FindResource("MultiSelectTreeViewListBoxItemSelectionContainerStyle") as Style;

            // Container Nodes (Directory)
            else
                return listBoxItem.FindResource("MultiSelectTreeViewListBoxItemContainerStyle") as Style;
        }
    }
}
