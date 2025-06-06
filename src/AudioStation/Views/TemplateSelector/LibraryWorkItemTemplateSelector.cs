using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels;

namespace AudioStation.Views.TemplateSelector
{
    public class LibraryWorkItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var presenter = container as ContentPresenter;
            var viewModel = item as LibraryWorkItemViewModel;

            if (viewModel == null || presenter == null)
                throw new NullReferenceException("Improper handling of LibraryWorkItemTemplateSelector");

            if (!viewModel.Completed)
                return presenter.FindResource("LibraryWorkItemDataTemplateNormal") as DataTemplate;

            //else if (!viewModel.Error && !viewModel.Completed)
            //    return presenter.FindResource("LibraryWorkItemDataTemplateNormal") as DataTemplate;

            else if (viewModel.Error)
                return presenter.FindResource("LibraryWorkItemDataTemplateError") as DataTemplate;

            else if (!viewModel.Error)
                return presenter.FindResource("LibraryWorkItemDataTemplateSuccess") as DataTemplate;

            else
                throw new Exception("Unknown Data Template (LibraryWorkItemTemplateSelector)");
        }
    }
}
