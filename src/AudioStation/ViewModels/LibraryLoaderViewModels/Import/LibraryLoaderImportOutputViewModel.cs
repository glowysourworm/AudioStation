using System.Collections.ObjectModel;

using AcoustID.Web;

using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    public class LibraryLoaderImportOutputViewModel : ViewModelBase, ILibraryLoaderImportOutput
    {
        string _destinationFolderBase;
        string _destinationPathCalculated;
        ObservableCollection<string> _logMessages;
        ObservableCollection<LookupResultViewModel> _acoustIDResults;
        ObservableCollection<MusicBrainzRecordingViewModel> _musicBrainzRecordingMatches;
        ObservableCollection<MusicBrainzCombinedLibraryEntryRecord> _musicBrainzCombinedRecords;
        MusicBrainzCombinedLibraryEntryRecord _finalQueryRecord;
        Mp3FileReference _importedRecord;

        MusicBrainzPicture? _bestFrontCover;
        MusicBrainzPicture? _bestBackCover;

        bool _acoustIDSuccess;
        bool _musicBrainzRecordingMatchSuccess;
        bool _musicBrainzCombinedRecordQuerySuccess;
        bool _tagEmbeddingSuccess;
        bool _mp3FileMoveSuccess;
        bool _mp3FileImportSuccess;

        public string DestinationFolderBase
        {
            get { return _destinationFolderBase; }
            set { RaiseAndSetIfChanged(ref _destinationFolderBase, value); }
        }
        public string DestinationPathCalculated
        {
            get { return _destinationPathCalculated; }
            set { RaiseAndSetIfChanged(ref _destinationPathCalculated, value); }
        }
        public ObservableCollection<string> LogMessages
        {
            get { return _logMessages; }
            set { RaiseAndSetIfChanged(ref _logMessages, value); }
        }
        public ObservableCollection<LookupResultViewModel> AcoustIDResults
        {
            get { return _acoustIDResults; }
            set { RaiseAndSetIfChanged(ref _acoustIDResults, value); }
        }
        public ObservableCollection<MusicBrainzRecordingViewModel> MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set { RaiseAndSetIfChanged(ref _musicBrainzRecordingMatches, value); }
        }
        public ObservableCollection<MusicBrainzCombinedLibraryEntryRecord> MusicBrainzCombinedRecords
        {
            get { return _musicBrainzCombinedRecords; }
            set { RaiseAndSetIfChanged(ref _musicBrainzCombinedRecords, value); }
        }
        public MusicBrainzCombinedLibraryEntryRecord FinalQueryRecord
        {
            get { return _finalQueryRecord; }
            set { RaiseAndSetIfChanged(ref _finalQueryRecord, value); }
        }
        public Mp3FileReference ImportedRecord
        {
            get { return _importedRecord; }
            set { RaiseAndSetIfChanged(ref _importedRecord, value); }
        }
        public MusicBrainzPicture? BestFrontCover
        {
            get { return _bestFrontCover; }
            set { RaiseAndSetIfChanged(ref _bestFrontCover, value); }
        }
        public MusicBrainzPicture? BestBackCover
        {
            get { return _bestBackCover; }
            set { RaiseAndSetIfChanged(ref _bestBackCover, value); }
        }
        public bool AcoustIDSuccess
        {
            get { return _acoustIDSuccess; }
            set { RaiseAndSetIfChanged(ref _acoustIDSuccess, value); }
        }
        public bool MusicBrainzRecordingMatchSuccess
        {
            get { return _musicBrainzRecordingMatchSuccess; }
            set { RaiseAndSetIfChanged(ref _musicBrainzRecordingMatchSuccess, value); }
        }
        public bool MusicBrainzCombinedRecordQuerySuccess
        {
            get { return _musicBrainzCombinedRecordQuerySuccess; }
            set { RaiseAndSetIfChanged(ref _musicBrainzCombinedRecordQuerySuccess, value); }
        }
        public bool TagEmbeddingSuccess
        {
            get { return _tagEmbeddingSuccess; }
            set { RaiseAndSetIfChanged(ref _tagEmbeddingSuccess, value); }
        }
        public bool Mp3FileMoveSuccess
        {
            get { return _mp3FileMoveSuccess; }
            set { RaiseAndSetIfChanged(ref _mp3FileMoveSuccess, value); }
        }
        public bool Mp3FileImportSuccess
        {
            get { return _mp3FileImportSuccess; }
            set { RaiseAndSetIfChanged(ref _mp3FileImportSuccess, value); }
        }


        #region (private / public) ILibraryLoaderImportOutput properties
        IEnumerable<LookupResult> ILibraryLoaderImportOutput.AcoustIDResults
        {
            get
            {
                return _acoustIDResults.Select(x => new LookupResult(x.Id.ToString(), x.Score, new Recording[]
                {
                    new Recording(0, x.MusicBrainzRecordingId.ToString(), string.Empty)

                })).ToList();
            }
            set
            {
                _acoustIDResults.Clear();
                foreach (var result in value)
                {
                    foreach (var recording in result.Recordings)
                    {
                        _acoustIDResults.Add(new LookupResultViewModel()
                        {
                            Id = new Guid(result.Id),
                            MusicBrainzRecordingId = new Guid(recording.Id),
                            Score = result.Score
                        });
                    }
                }

                OnPropertyChanged("AcoustIDResults");
            }
        }
        IEnumerable<MusicBrainzRecording> ILibraryLoaderImportOutput.MusicBrainzRecordingMatches
        {
            get
            {
                return _musicBrainzRecordingMatches.Select(ApplicationHelpers.Map<MusicBrainzRecordingViewModel, MusicBrainzRecording>).ToList();
            }
            set
            {
                _musicBrainzRecordingMatches.Clear();
                _musicBrainzRecordingMatches.AddRange(value.Select(ApplicationHelpers.Map<MusicBrainzRecording, MusicBrainzRecordingViewModel>));

                OnPropertyChanged("MusicBrainzRecordingMatches");
            }
        }
        IEnumerable<MusicBrainzCombinedLibraryEntryRecord> ILibraryLoaderImportOutput.MusicBrainzCombinedLibraryEntryRecords
        {
            get { return _musicBrainzCombinedRecords; }
            set
            {
                _musicBrainzCombinedRecords.Clear();
                _musicBrainzCombinedRecords.AddRange(value);

                OnPropertyChanged("MusicBrainzCombinedLibraryEntryRecords");
            }
        }
        #endregion

        public LibraryLoaderImportOutputViewModel()
        {
            this.AcoustIDResults = new ObservableCollection<LookupResultViewModel>();
            this.MusicBrainzCombinedRecords = new ObservableCollection<MusicBrainzCombinedLibraryEntryRecord>();
            this.MusicBrainzRecordingMatches = new ObservableCollection<MusicBrainzRecordingViewModel>();
            this.LogMessages = new ObservableCollection<string>();
        }
    }
}
