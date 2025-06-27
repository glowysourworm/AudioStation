using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.LogViewModels;

using Microsoft.Extensions.Logging;

namespace AudioStation.Views.TemplateSelector
{
    public class LogItemContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var presenter = container as ListBoxItem;
            var viewModel = item as LogMessageViewModel;

            if (viewModel == null || presenter == null)
                throw new NullReferenceException("Improper handling of LogItemTemplateSelector");

            switch (viewModel.Level)
            {
                case LogLevel.Trace:
                    return presenter.FindResource("LogItemContainerStyleTrace") as Style;
                case LogLevel.Debug:
                    return presenter.FindResource("LogItemContainerStyleDebug") as Style;
                case LogLevel.Information:
                    return presenter.FindResource("LogItemContainerStyleInfo") as Style;
                case LogLevel.Warning:
                    return presenter.FindResource("LogItemContainerStyleWarning") as Style;
                case LogLevel.Error:
                    return presenter.FindResource("LogItemContainerStyleError") as Style;
                case LogLevel.Critical:
                    return presenter.FindResource("LogItemContainerStyleCritical") as Style;
                case LogLevel.None:
                    return presenter.FindResource("LogItemContainerStyle") as Style;
                default:
                    throw new Exception("Unhandled log level:  LogItemContainerStyleSelector.cs");
            }
        }
    }
}
