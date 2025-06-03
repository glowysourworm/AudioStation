using System.Windows;

using AudioStation.Controller;
using AudioStation.ViewModels;

namespace AudioStation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // TODO: IocFramework 

            var dialogController = new DialogController();
            var audioController = new AudioController();
            this.DataContext = new MainViewModel(dialogController, audioController);
        }
    }
}