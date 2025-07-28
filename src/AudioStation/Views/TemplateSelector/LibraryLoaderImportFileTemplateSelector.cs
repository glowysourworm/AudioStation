using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.LibraryLoaderViewModels.Import;

namespace AudioStation.Views.TemplateSelector
{
    public class LibraryLoaderImportFileTemplateSelector : DataTemplateSelector
    {
        public LibraryLoaderImportFileTemplateSelector()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var viewModel = item as LibraryLoaderImportFileViewModel;
            var contentPresenter = container as ContentPresenter;

            if (viewModel != null && contentPresenter != null)
            {
                if (viewModel.IsDirectory)
                    return contentPresenter.FindResource("LibraryLoaderImportDirectoryTemplate") as DataTemplate;

                else
                    return contentPresenter.FindResource("LibraryLoaderImportFileTemplate") as DataTemplate;
            }
            else
                throw new Exception("Invalid use of LibraryLoaderImportFileTemplateSelector");
        }
    }
}
