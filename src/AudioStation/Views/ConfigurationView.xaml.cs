using System.Windows.Controls;

using AudioStation.Core;
using AudioStation.ViewModels;

namespace AudioStation.Views
{
    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();

            this.DataContextChanged += ConfigurationView_DataContextChanged;
        }

        private void ConfigurationView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (this.DataContext as MainViewModel);

            if (viewModel != null)
            {
                this.PasswordTB.Password = viewModel.Configuration.DatabasePassword;
            }
        }

        private void PasswordTB_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as MainViewModel);

            if (viewModel != null)
            {
                viewModel.Configuration.DatabasePassword = this.PasswordTB.Password;
            }
        }
    }
}
