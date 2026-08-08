using System.Windows;
using System.Windows.Controls;

using AudioStation.Controls;

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

            var viewModel = listBoxItem.DataContext as MultiSelectTreeItemViewModel;

            if (viewModel == null)
                return null;

            // Leaf Nodes (File)
            if (!viewModel.CanHaveChildren)
                return listBoxItem.FindResource("MultiSelectTreeViewListBoxItemSelectionContainerStyle") as Style;

            // Container Nodes (Directory)
            else
                return listBoxItem.FindResource("MultiSelectTreeViewListBoxItemContainerStyle") as Style;
        }
    }
}
