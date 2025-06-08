using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Component;
using AudioStation.ViewModels;

namespace AudioStation.Views.TemplateSelector
{
    public class LibraryWorkItemContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var presenter = container as ListBoxItem;
            var viewModel = item as LibraryWorkItemViewModel;

            if (viewModel == null || presenter == null)
                throw new NullReferenceException("Improper handling of LogItemTemplateSelector");

            switch (viewModel.LoadState)
            {
                case LibraryWorkItemState.Pending:
                    return presenter.FindResource("LibraryWorkItemContainerStylePending") as Style;
                case LibraryWorkItemState.Processing:
                    return presenter.FindResource("LibraryWorkItemContainerStyleNormal") as Style;
                case LibraryWorkItemState.CompleteSuccessful:
                    return presenter.FindResource("LibraryWorkItemContainerStyleSuccess") as Style;
                case LibraryWorkItemState.CompleteError:
                    return presenter.FindResource("LibraryWorkItemContainerStyleError") as Style;
                default:
                    throw new Exception("Unknown Style (LibraryWorkItemContainerStyleSelector)");
            }
        }
    }
}
