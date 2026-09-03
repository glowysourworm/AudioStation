using System.Windows.Controls;

using AudioStation.Event;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportConfigurationView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportConfigurationView(IIocEventAggregator eventAggregator)
        {
            InitializeComponent();

            // Initial Configuration
            this.LibraryDirectoriesView.ItemsSource = eventAggregator.GetEvent<ConfigurationEvent>().GetLast()?.ViewModel?.LibraryDirectories;

            // Configuration
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                this.LibraryDirectoriesView.ItemsSource = eventData.ViewModel.LibraryDirectories;
            });
        }
    }
}
