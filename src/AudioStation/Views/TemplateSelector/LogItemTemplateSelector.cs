using System.Windows;
using System.Windows.Controls;

using AudioStation.Model;
using AudioStation.ViewModels;

using Microsoft.Extensions.Logging;

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

            if (viewModel.Level <= LogLevel.Information)
                return presenter.FindResource("LogDataTemplateInfo") as DataTemplate;

            else if (viewModel.Level <= LogLevel.Warning)
                return presenter.FindResource("LogDataTemplateWarning") as DataTemplate;

            // ALL ERRORS
            else if (viewModel.Level <= LogLevel.None)
                return presenter.FindResource("LogDataTemplateError") as DataTemplate;

            else
                throw new Exception("Unknown Data Template (LogItemTemplateSelector)");
        }
    }
}
