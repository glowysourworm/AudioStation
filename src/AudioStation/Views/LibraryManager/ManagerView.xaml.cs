using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Model;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;
using AudioStation.Views.LibraryEntryViews;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

using TagLib;

namespace AudioStation.Views.LibraryManager
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IOutputController _outputController;

        private ObservableCollection<TabItemPressable> _tabItems;

        [IocImportingConstructor]
        public ManagerView(IMusicBrainzClient musicBrainzClient, IOutputController outputController)
        {
            _musicBrainzClient = musicBrainzClient;
            _outputController = outputController;
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

        private async void OnLibraryEntryTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                        _tabItems.Add(await CreateLibraryEntryFileTab(tabViewModel));
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
        private async Task<TabItemPressable> CreateLibraryEntryFileTab(LibraryEntryViewModel viewModel)
        {
            var tabItem = new TabItemPressable();
            tabItem.Style = App.Current.Resources["ManagerTabItemStyle"] as Style;
            tabItem.Header = GetFileTabName(viewModel);
            tabItem.DataContext = viewModel;

            // Entry View
            var view = new EntryView();

            // Replicate TagLib's tag into our data structure
            var tagFile = TagLib.File.Create(viewModel.FileName);
            var tag = tagFile.Tag;
            var tagFileViewModel = new FileViewModel(tagFile);

            // Hand off the tag data to the view
            view.TagFileView.DataContext = tagFileViewModel;                                     // Entire File (combined tag)
            view.Id3v1TagView.DataContext = new TagViewModel(tagFile.GetTag(TagTypes.Id3v1));    // Tag piece (will have to sync data)
            view.Id3v2TagView.DataContext = new TagViewModel(tagFile.GetTag(TagTypes.Id3v2));    // Tag piece (will have to sync data)

            // Listen for Music Brainz Query
            view.TagFileView.FindOnMusicBrainzEvent += async () =>
            {
                if (tagFileViewModel.Tag.MusicBrainzArtistId == null ||
                    tagFileViewModel.Tag.MusicBrainzReleaseId == null)
                    return;

                var artistId = new Guid(tagFileViewModel.Tag.MusicBrainzArtistId);
                var releaseId = new Guid(tagFileViewModel.Tag.MusicBrainzReleaseId);
                var trackId = new Guid(tagFileViewModel.Tag.MusicBrainzTrackId);

                var musicBrainzViewModel = await _musicBrainzClient.GetCombinedData(releaseId, artistId, trackId, tagFileViewModel.Tag.Title);

                if (musicBrainzViewModel != null)
                {
                    // Music Brainz API:  Artist URL relationships have many different links to browse. Also, ASIN is the
                    //                    Amazon Id (also), or some part of their API.
                    //
                    //                    Found a Track Id that was out of date. The artist / recording Ids may be ok to 
                    //                    update the tag information with.

                    view.MusicBrainzTab.IsEnabled = true;
                    view.MusicBrainzView.DataContext = musicBrainzViewModel;
                }
                else
                {
                    _outputController.Log("Music Brainz Client Failed:  {0}", LogMessageType.General, LogLevel.Information, tagFileViewModel.Name);
                }
            };


            tabItem.Content = view;

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
