using System.ComponentModel;
using System.Windows;

using AudioStation.Controller;
using AudioStation.Core.Component;
using AudioStation.ViewModels;

namespace AudioStation
{
    /// <summary>
    /// IDisposable pattern implemented by Application object OnExit(..)
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // TODO: IocFramework 

            var dialogController = new DialogController();
            var audioController = new AudioController();
            var modelController = new ModelController(new Configuration());
            var libraryLoader = new LibraryLoader();
            this.DataContext = new MainViewModel(dialogController, audioController, modelController, libraryLoader);
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