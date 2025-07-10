using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Event;
using AudioStation.Event.EventViewModel;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

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

        [IocImportingConstructor]
        public LibraryLoaderImportFileView(IIocEventAggregator eventAggregator, 
                                           IDialogController dialogController, 
                                           IAcoustIDClient acoustIDClient)
        {
            _eventAggregator = eventAggregator;
            _dialogController = dialogController;
            _acoustIDClient = acoustIDClient;

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
                    // Loading...
                    _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Running AcoustID Fingerprinting Service..."));

                    var acoustIDResults = await _acoustIDClient.IdentifyFingerprint(selectedFile.FileFullPath, 30);

                    selectedFile.ImportOutput.AcoustIDResults.Clear();
                    selectedFile.ImportOutput.AcoustIDResults.AddRange(acoustIDResults.SelectMany(result =>
                    {
                        var format = "Acoust ID Result: (All Id's refer to Music Brainz Records) \n Id={0} \n Score={1:P2} \n RecordingId={2} \n Recording=({3})";
                        var recordingFormat = "Title={0} Artist={1} Album={2}";

                        return result.Recordings.Select(recording =>
                        {
                            var recordingResult = string.Format(recordingFormat, 
                                                                recording.Title ?? "(Unknown)", 
                                                                recording.Artists?.FirstOrDefault()?.Name ?? "(Unknown)", 
                                                                recording.Releases?.FirstOrDefault()?.Title ?? "(Unknown)");


                            return string.Format(format, result.Id, result.Score, recording.Id, recordingResult);
                        });
                    }));
                    selectedFile.ImportOutput.AcoustIDSuccess = selectedFile.ImportOutput.AcoustIDResults.Any();

                    // (Loading Close)
                    _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

                    // Show AcoustID Results
                    _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData("Acoust ID Results (Min Score = 30%)", 
                                                                                         new DialogMessageListViewModel()
                    {
                        MessageList = selectedFile.ImportOutput.AcoustIDResults
                    }));
                }                
            }
        }

        private void MusicBrainzTestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                // TODO
            }
        }
    }
}
