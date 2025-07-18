﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryLoaderViews
{
    [IocExportDefault]
    public partial class LibraryLoaderImportFileView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IDialogController _dialogController;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IMusicBrainzClient _musicBrainzClient;

        [IocImportingConstructor]
        public LibraryLoaderImportFileView(IIocEventAggregator eventAggregator,
                                           IDialogController dialogController,
                                           IAcoustIDClient acoustIDClient,
                                           IMusicBrainzClient musicBrainzClient)
        {
            _eventAggregator = eventAggregator;
            _dialogController = dialogController;
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient = musicBrainzClient;

            InitializeComponent();
        }

        private void ImportLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null)
            {
                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsSelected = this.ImportLB.SelectedItems.Contains(item);
                }
            }
        }

        private void InputFileExpanderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsExpanded = (selectedFile == item) && selectedFile.IsExpanded;
                }
            }
        }

        private void EditTagButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                if (selectedFile != null)
                {
                    var tagFileGroupModel = new TagFileGroupViewModel(new TagFileViewModel[] { selectedFile.TagFile });

                    _dialogController.ShowTagWindow(tagFileGroupModel);
                }
            }
        }

        private async void AcoustIDTestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                if (selectedFile != null)
                {
                    await RunAcoustID(selectedFile, true);
                }
            }
        }

        private async void MusicBrainzTestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                if (selectedFile != null)
                {
                    // Must first run AcoustID
                    if (!selectedFile.ImportOutput.AcoustIDSuccess)
                        await RunAcoustID(selectedFile, false);

                    // AcoustID Failure
                    if (!selectedFile.ImportOutput.AcoustIDSuccess)
                    {
                        _dialogController.ShowAlert("AcoustID Fingerprinting Failed", "AcoustID was not successful in finidng your audio file. Please use the tag editor to complete the import.");
                    }

                    // AcoustID Success -> Music Brainz
                    else
                    {
                        RunMusicBrainz(selectedFile);
                    }
                }
            }
        }

        private async Task RunAcoustID(LibraryLoaderImportFileViewModel selectedFile, bool showResults)
        {
            if (!selectedFile.ImportOutput.AcoustIDSuccess)
            {
                // Loading...
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Running AcoustID Fingerprinting Service..."));

                var acoustIDResults = await _acoustIDClient.IdentifyFingerprint(selectedFile.FileFullPath, 30);

                selectedFile.ImportOutput.AcoustIDResults.Clear();
                selectedFile.ImportOutput.AcoustIDResults.AddRange(acoustIDResults.SelectMany(result =>
                {
                    // Recording data is not filled out by the lookup service. Just the ID's.
                    return result.Recordings.Select(recording => new LookupResultViewModel()
                    {
                        Id = new Guid(result.Id),
                        Score = result.Score,
                        MusicBrainzRecordingId = new Guid(recording.Id)
                    });
                }));

                selectedFile.ImportOutput.AcoustIDSuccess = selectedFile.ImportOutput.AcoustIDResults.Any();

                // (Loading Close)
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            }

            if (showResults)
            {
                ShowAcoustIDResults(selectedFile);
            }
        }

        private void RunMusicBrainz(LibraryLoaderImportFileViewModel selectedFile)
        {
            if (!selectedFile.ImportOutput.MusicBrainzRecordingMatchSuccess)
            {
                // Loading...
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Running Music Brainz Client..."));

                var musicBrainzRecordings = selectedFile.ImportOutput
                                                        .AcoustIDResults
                                                        .Select(x => _musicBrainzClient.GetRecordingById(x.MusicBrainzRecordingId).Result);

                selectedFile.ImportOutput.MusicBrainzRecordingMatches.Clear();

                foreach (var recording in musicBrainzRecordings)
                {
                    try
                    {
                        // AutoMapper: MusicBrainzRecording -> (view model)
                        var viewModel = ApplicationHelpers.Map<MusicBrainzRecording, MusicBrainzRecordingViewModel>(recording);

                        selectedFile.ImportOutput.MusicBrainzRecordingMatches.Add(viewModel);
                    }
                    catch (Exception ex)
                    {
                        ApplicationHelpers.Log("Music Brainz Recording Mapping Error:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                    }
                }

                selectedFile.ImportOutput.MusicBrainzRecordingMatchSuccess = selectedFile.ImportOutput.MusicBrainzRecordingMatches.Any();

                // (Loading Close)
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            }

            ShowMusicBrainzResults(selectedFile);
        }

        private void ShowAcoustIDResults(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Format AcoustID Output
            string format = "Id={0}\nScore={1:P2}\nMusic Brainz Id={2}";

            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData("Acoust ID Results (Min Score = 30%)",
                                                                new DialogMessageListViewModel()
                                                                {
                                                                    MessageList = new ObservableCollection<string>(
                                                                                  selectedFile.ImportOutput
                                                                                              .AcoustIDResults
                                                                                              .Select(x => string.Format(format, x.Id, x.Score, x.MusicBrainzRecordingId)))
                                                                }));
        }

        private void ShowMusicBrainzResults(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Format Music Brainz Output
            string format = "Id={0}\nArtist={1}\nAlbum={2}\nTrack={3}";

            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData("Music Brainz Results",
                                                                new DialogMessageListViewModel()
                                                                {
                                                                    MessageList = new ObservableCollection<string>(
                                                                                  selectedFile.ImportOutput
                                                                                              .MusicBrainzRecordingMatches
                                                                                              .Select(x => string.Format(format, x.Id,
                                                                                                                                 x.ArtistCredit?.FirstOrDefault()?.Name ?? string.Empty,
                                                                                                                                 x.Releases?.FirstOrDefault()?.Title ?? string.Empty,
                                                                                                                                 x.Title ?? string.Empty)))
                                                                }));
        }
    }
}
