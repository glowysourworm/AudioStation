using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Component.LibraryLoaderComponent;
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

            switch (viewModel.LoadState)
            {
                case LibraryWorkItemState.Pending:
                    return presenter.FindResource("LibraryWorkItemDataTemplatePending") as DataTemplate;
                case LibraryWorkItemState.Processing:
                    return presenter.FindResource("LibraryWorkItemDataTemplateNormal") as DataTemplate;
                case LibraryWorkItemState.CompleteSuccessful:
                    return presenter.FindResource("LibraryWorkItemDataTemplateSuccess") as DataTemplate;
                case LibraryWorkItemState.CompleteError:
                    return presenter.FindResource("LibraryWorkItemDataTemplateError") as DataTemplate;
                default:
                    throw new Exception("Unknown Data Template (LibraryWorkItemTemplateSelector)");
            }
        }
    }
}
