using System.Windows.Controls;

using AudioStation.Core.Utility.FileUtility;
using AudioStation.Event;
using AudioStation.ViewModels;
using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportConfigurationView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;

        AudioStationConfigurationViewModel _configuration;

        [IocImportingConstructor]
        public LibraryImportConfigurationView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();

            // Initial Configuration
            this.LibraryDirectoriesView.ItemsSource = eventAggregator.GetEvent<ConfigurationEvent>().GetLast()?.ViewModel?.LibraryDirectories;

            // Configuration
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                _configuration = eventData.ViewModel;

                this.LibraryDirectoriesView.ItemsSource = eventData.ViewModel.LibraryDirectories;
                this.LibraryDirectoriesCB1.ItemsSource = eventData.ViewModel.LibraryDirectories;
                this.LibraryDirectoriesCB2.ItemsSource = eventData.ViewModel.LibraryDirectories;
            });
        }

        private void AddDirectoryButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // -> Add Library Directory
            //
            var defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            _configuration.LibraryDirectories.Add(new LibraryDirectoryViewModel()
            {
                Directory = defaultDirectory,
                DirectoryLabel = FileHelpers.CreateUniqueLabel("New Directory", _configuration.LibraryDirectories.Select(x => x.DirectoryLabel).ToArray())
            });

            _eventAggregator.GetEvent<ConfigurationEvent>().Publish(new ConfigurationEventData()
            {
                ViewModel = _configuration,
                Type = ConfigurationEventType.ModifyRequest
            });
        }
    }
}
