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
            var listBoxItem = container as ListBoxItem;

            if (listBoxItem == null)
                throw new NullReferenceException("Improper handling of LibraryLoaderImportTreeStyleSelector");

            var viewModel = listBoxItem.DataContext as LibraryLoaderImportTreeViewModel;

            if (viewModel == null)
                return null;

            if (viewModel.NodeValue.IsDirectory)
            {
                return listBoxItem.FindResource("LibraryImportTreeDirectoryItemContainerStyle") as Style;
            }
                

            else
                return listBoxItem.FindResource("LibraryImportTreeFileItemContainerStyle") as Style;
        }
    }
}
