using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportOutputViewModel : ViewModelBase
    {
        string _importFileName;
        string _outputFileName;
        ObservableCollection<string> _logMessages;
        ObservableCollection<string> _acoustIDResults;
        ObservableCollection<string> _musicBrainzRecordingMatches;
        ObservableCollection<string> _musicBrainzCombinedRecords;
        string _finalRecord;
        string _importedRecord;

        bool _acoustIDSuccess;
        bool _musicBrainzRecordingMatchSuccess;
        bool _musicBrainzCombinedRecordQuerySuccess;
        bool _tagEmbeddingSuccess;
        bool _importFileMoveSuccess;
        bool _importSuccess;

        public string ImportFileName
        {
            get { return _importFileName; }
            set { this.RaiseAndSetIfChanged(ref _importFileName, value); }
        }
        public string OutputFileName
        {
            get { return _outputFileName; }
            set { this.RaiseAndSetIfChanged(ref _outputFileName, value); }
        }
        public ObservableCollection<string> LogMessages
        {
            get { return _logMessages; }
            set { this.RaiseAndSetIfChanged(ref _logMessages, value); }
        }
        public ObservableCollection<string> AcoustIDResults
        {
            get { return _acoustIDResults; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDResults, value); }
        }
        public ObservableCollection<string> MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingMatches, value); }
        }
        public ObservableCollection<string> MusicBrainzCombinedRecords
        {
            get { return _musicBrainzCombinedRecords; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzCombinedRecords, value); }
        }
        public string FinalRecord
        {
            get { return _finalRecord; }
            set { this.RaiseAndSetIfChanged(ref _finalRecord, value); }
        }
        public string ImportedRecord
        {
            get { return _importedRecord; }
            set { this.RaiseAndSetIfChanged(ref _importedRecord, value); }
        }
        public bool AcoustIDSuccess
        {
            get { return _acoustIDSuccess; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDSuccess, value); }
        }
        public bool MusicBrainzRecordingMatchSuccess
        {
            get { return _musicBrainzRecordingMatchSuccess; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingMatchSuccess, value); }
        }
        public bool MusicBrainzCombinedRecordQuerySuccess
        {
            get { return _musicBrainzCombinedRecordQuerySuccess; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzCombinedRecordQuerySuccess, value); }
        }
        public bool TagEmbeddingSuccess
        {
            get { return _tagEmbeddingSuccess; }
            set { this.RaiseAndSetIfChanged(ref _tagEmbeddingSuccess, value); }
        }
        public bool ImportFileMoveSuccess
        {
            get { return _importFileMoveSuccess; }
            set { this.RaiseAndSetIfChanged(ref _importFileMoveSuccess, value); }
        }
        public bool ImportSuccess
        {
            get { return _importSuccess; }
            set { this.RaiseAndSetIfChanged(ref _importSuccess, value); }
        }

        public LibraryLoaderImportOutputViewModel()
        {
            this.AcoustIDResults = new ObservableCollection<string>();
            this.MusicBrainzCombinedRecords = new ObservableCollection<string>();
            this.MusicBrainzRecordingMatches = new ObservableCollection<string>();
            this.LogMessages = new ObservableCollection<string>();
        }
    }
}
