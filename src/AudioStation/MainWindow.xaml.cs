using System.ComponentModel;
using System.Windows;

using AudioStation.Controller;
using AudioStation.Core.Component;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation
{
    /// <summary>
    /// IDisposable pattern implemented by Application object OnExit(..)
    /// </summary>
    [IocExportDefault]
    public partial class MainWindow : Window
    {
        // Needed by the framework
        public MainWindow()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.DataContext = mainViewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.Dispose();
            }

            base.OnClosing(e);
        }
    }
}