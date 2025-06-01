using AudioStation.Model;
using AudioStation.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioStation;

public partial class ManagerView : UserControl
{
    public ManagerView()
    {
        InitializeComponent();

        this.Loaded += ManagerView_Loaded;
    }

    private void ManagerView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // BINDING NOT WORKING!!!!!
        this.LibraryLB.ItemsSource = (this.DataContext as MainViewModel).Library.AllTitles;
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var item = e.AddedItems[0] as LibraryEntry;

            this.LocalEntryItemView.DataContext = item;
            this.MusicBrainzItemView.DataContext = item;
        }
    }
}