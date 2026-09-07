using System.Collections;
using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels;
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
            DependencyProperty.Register("ConfigurationLocked", typeof(bool), typeof(ConfigurationLibraryDirectoriesView), new PropertyMetadata(OnChanged));

        public static readonly DependencyProperty IsApplicationDirectoryViewProperty =
            DependencyProperty.Register("IsApplicationDirectoryView", typeof(bool), typeof(ConfigurationLibraryDirectoriesView), new PropertyMetadata(OnChanged));

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

        public bool IsApplicationDirectoryView
        {
            get { return (bool)GetValue(IsApplicationDirectoryViewProperty); }
            set { SetValue(IsApplicationDirectoryViewProperty, value); }
        }

        public ConfigurationLibraryDirectoriesView()
        {
            InitializeComponent();
        }

        private void Update()
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                // Update columns by hand (binding proxy has issues)
                foreach (var column in this.LibraryFoldersDG.Columns)
                {
                    column.IsReadOnly = viewModel.ConfigurationLocked;

                    if (column.Header?.Equals("Primary") ?? false)
                    {
                        column.Visibility = !this.IsApplicationDirectoryView ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                // Update row details visibility (this binding was finiky)
                if (this.IsApplicationDirectoryView)
                    this.LibraryFoldersDG.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;

                else
                    this.LibraryFoldersDG.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ConfigurationLibraryDirectoriesView;

            if (control != null)
                control.Update();
        }
    }
}
