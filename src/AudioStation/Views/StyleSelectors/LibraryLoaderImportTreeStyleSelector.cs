using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
using AudioStation.ViewModels.LogViewModels;

using Microsoft.Extensions.Logging;

namespace AudioStation.Views.StyleSelectors
{
    public class LibraryLoaderImportTreeStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var treeViewItem = container as TreeViewItem;

            if (treeViewItem == null)
                throw new NullReferenceException("Improper handling of LibraryLoaderImportTreeStyleSelector");

            var viewModel = treeViewItem.DataContext as LibraryLoaderImportTreeViewModel;

            if (viewModel == null)
                throw new NullReferenceException("Improper data binding for LibraryLoaderImportTreeStyleSelector");

            if (viewModel.NodeValue.IsDirectory)
                return treeViewItem.FindResource("LibraryImportTreeDirectoryItemContainerStyle") as Style;

            else
                return treeViewItem.FindResource("LibraryImportTreeFileItemContainerStyle") as Style;
        }
    }
}
