using AudioStation.ViewModels;

using Avalonia.Controls;

namespace AudioStation.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        this.DataContextChanged += MainView_DataContextChanged;
    }

    private void MainView_DataContextChanged(object? sender, System.EventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;

        if (viewModel != null)
        {
            // Bindings weren't working (tried ViewLocator, CompiledBindings)
            this.ConfigurationView.DataContext = viewModel.Configuration;
            this.ManagerView.DataContext = viewModel;
            this.NowPlayingView.DataContext = viewModel.Library;
            this.OutputLB.ItemsSource = viewModel.OutputMessages;
        }
    }
}