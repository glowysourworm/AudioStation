using System.Windows;
using System.Windows.Controls;

using AudioStation.Model;
using AudioStation.ViewModels;

namespace AudioStation.Views.TemplateSelector
{
    public class LogItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var presenter = container as ContentPresenter;
            var viewModel = item as LogMessageViewModel;

            if (viewModel == null || presenter == null)
                throw new NullReferenceException("Improper handling of LogItemTemplateSelector");

            if (viewModel.Severity == LogMessageSeverity.Info)
                return presenter.FindResource("LogDataTemplateInfo") as DataTemplate;

            else if (viewModel.Severity == LogMessageSeverity.Warning)
                return presenter.FindResource("LogDataTemplateWarning") as DataTemplate;

            else if (viewModel.Severity == LogMessageSeverity.Error)
                return presenter.FindResource("LogDataTemplateError") as DataTemplate;

            else
                throw new Exception("Unknown Data Template (LogItemTemplateSelector)");
        }
    }
}
