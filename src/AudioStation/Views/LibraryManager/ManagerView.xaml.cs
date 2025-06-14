using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using AudioStation.Component;
using AudioStation.Controls;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.Views.LibraryEntryViews;
using AudioStation.Views.VendorEntryViews;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryManager
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        private ObservableCollection<TabItemPressable> _tabItems;

        [IocImportingConstructor]
        public ManagerView()
        {
            _tabItems = new ObservableCollection<TabItemPressable>();

            InitializeComponent();
            InitializeTabs();

            this.DataContextChanged += ManagerView_DataContextChanged;

            // Have to divide the tab items into:  Non-closeable / Closeable
            this.ManagerTabControl.ItemsSource = _tabItems;
        }

        private void ManagerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldViewModel = e.OldValue as LibraryViewModel;
            var newViewModel = e.NewValue as LibraryViewModel;

            if (oldViewModel != null)
                oldViewModel.LibraryEntryTabItems.CollectionChanged -= OnLibraryEntryTabsChanged;

            if (newViewModel != null)
                newViewModel.LibraryEntryTabItems.CollectionChanged += OnLibraryEntryTabsChanged;
        }

        private void OnLibraryEntryTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            // Tab Item Header:  Set the header here because it's used to reference the view model
            //
            if (viewModel != null)
            {
                // Removed Tab(s)
                for (int index = _tabItems.Count - 1; index >= 2 /* Skipping Non-Closeable Tabs */; index--)
                {
                    var entryViewModel = viewModel.LibraryEntryTabItems.FirstOrDefault(x => GetFileTabName(x) == (string)_tabItems[index].Header);

                    if (entryViewModel == null)
                        _tabItems.RemoveAt(index);
                }

                // Sync the tab items w/ the view model
                foreach (var tabViewModel in viewModel.LibraryEntryTabItems)
                {
                    var tabItem = _tabItems.FirstOrDefault(x => (string)x.Header == GetFileTabName(tabViewModel));

                    if (tabItem == null)
                        _tabItems.Add(CreateLibraryEntryFileTab(tabViewModel));
                }
            }
        }

        // Gets a consistent readable tab item header / name
        private string GetFileTabName(LibraryEntryViewModel entryViewModel)
        {
            return string.Format("File ({0})", entryViewModel.Id.ToString());
        }

        private void InitializeTabs()
        {
            // Using UserControl resources "proxy" to utilize all the XAML binding
            _tabItems.Add(CreateLibraryDatabaseTab());
            _tabItems.Add(CreateLibraryFileTab());
        }

        private TabItemPressable CreateLibraryDatabaseTab()
        {
            return this.Resources["LibraryDatabaseTab"] as TabItemPressable;
        }
        private TabItemPressable CreateLibraryFileTab()
        {
            return this.Resources["LibraryFileTab"] as TabItemPressable;
        }
        private TabItemPressable CreateLibraryEntryFileTab(LibraryEntryViewModel viewModel)
        {
            var tabItem = new TabItemPressable();
            tabItem.Style = App.Current.Resources["ManagerTabItemStyle"] as Style;
            tabItem.Header = GetFileTabName(viewModel);
            tabItem.Content = new EntryView();
            tabItem.DataContext = viewModel;

            // TODO: Can't bind because of command parameter.
            tabItem.CloseCommand = new SimpleCommand(() =>
            {
                var libraryViewModel = this.DataContext as LibraryViewModel;

                libraryViewModel?.RemoveLibraryEntryTabCommand.Execute(viewModel);
            });
            return tabItem;
        }
    }
}
