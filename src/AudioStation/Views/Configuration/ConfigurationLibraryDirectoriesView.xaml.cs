using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Views.Configuration
{
    public partial class ConfigurationLibraryDirectoriesView : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ConfigurationLibraryDirectoriesView));

        public static readonly DependencyProperty ConfigurationLockedProperty =
            DependencyProperty.Register("ConfigurationLocked", typeof(bool), typeof(ConfigurationLibraryDirectoriesView));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
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
