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

            switch (viewModel.Level)
            {
                case LogLevel.Trace:
                    return presenter.FindResource("LogDataTemplateTrace") as DataTemplate;
                case LogLevel.Debug:
                    return presenter.FindResource("LogDataTemplateDebug") as DataTemplate;
                case LogLevel.Information:
                    return presenter.FindResource("LogDataTemplateInfo") as DataTemplate;
                case LogLevel.Warning:
                    return presenter.FindResource("LogDataTemplateWarning") as DataTemplate;
                case LogLevel.Error:
                    return presenter.FindResource("LogDataTemplateError") as DataTemplate;
                case LogLevel.Critical:
                    return presenter.FindResource("LogDataTemplateCritical") as DataTemplate;
                case LogLevel.None:
                    return presenter.FindResource("LogDataTemplate") as DataTemplate;
                default:
                    throw new Exception("Unhandled log level:  LogItemTemplateSelector.cs");
            }
        }
    }
}
