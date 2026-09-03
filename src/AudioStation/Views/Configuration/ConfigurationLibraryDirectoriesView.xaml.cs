using System.Collections;
using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.MainViewModels;

namespace AudioStation.Views.Configuration
{
    public partial class ConfigurationLibraryDirectoriesView : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ConfigurationLibraryDirectoriesView));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(LibraryDirectoryViewModel), typeof(ConfigurationLibraryDirectoriesView));

        public static readonly DependencyProperty ConfigurationLockedProperty =
            DependencyProperty.Register("ConfigurationLocked", typeof(bool), typeof(ConfigurationLibraryDirectoriesView));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public LibraryDirectoryViewModel SelectedItem
        {
            get { return (LibraryDirectoryViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public bool ConfigurationLocked
        {
            get { return (bool)GetValue(ConfigurationLockedProperty); }
            set { SetValue(ConfigurationLockedProperty, value); }
        }

        public ConfigurationLibraryDirectoriesView()
        {
            InitializeComponent();
        }
    }
}
