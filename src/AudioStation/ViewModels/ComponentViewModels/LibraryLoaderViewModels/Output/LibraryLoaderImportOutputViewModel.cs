using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent.Output.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.ViewModels.TagViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderImportOutputViewModel : ViewModelBase, ILibraryLoaderImportOutput
    {
        string _destinationFolderBase;
        string _destinationPathCalculated;
        ObservableCollection<string> _logMessages;
        ObservableCollection<AcoustIDLookupResultViewModel> _acoustIDResults;
        ObservableCollection<TagSmallViewModel> _musicBrainzRecordingMatches;
        //ObservableCollection<MusicBrainzCombinedLibraryEntryRecord> _musicBrainzCombinedRecords;
        //MusicBrainzCombinedLibraryEntryRecord _finalQueryRecord;
        Track _importedRecord;

        //MusicBrainzPicture? _bestFrontCover;
        //MusicBrainzPicture? _bestBackCover;

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
        public ObservableCollection<AcoustIDLookupResultViewModel> AcoustIDResults
        {
            get { return _acoustIDResults; }
            set { RaiseAndSetIfChanged(ref _acoustIDResults, value); }
        }
        public ObservableCollection<TagSmallViewModel> MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set { RaiseAndSetIfChanged(ref _musicBrainzRecordingMatches, value); }
        }
        //public ObservableCollection<MusicBrainzCombinedLibraryEntryRecord> MusicBrainzCombinedRecords
        //{
        //    get { return _musicBrainzCombinedRecords; }
        //    set { RaiseAndSetIfChanged(ref _musicBrainzCombinedRecords, value); }
        //}
        //public MusicBrainzCombinedLibraryEntryRecord FinalQueryRecord
        //{
        //    get { return _finalQueryRecord; }
        //    set { RaiseAndSetIfChanged(ref _finalQueryRecord, value); }
        //}
        public Track ImportedRecord
        {
            get { return _importedRecord; }
            set { RaiseAndSetIfChanged(ref _importedRecord, value); }
        }
        //public MusicBrainzPicture? BestFrontCover
        //{
        //    get { return _bestFrontCover; }
        //    set { RaiseAndSetIfChanged(ref _bestFrontCover, value); }
        //}
        //public MusicBrainzPicture? BestBackCover
        //{
        //    get { return _bestBackCover; }
        //    set { RaiseAndSetIfChanged(ref _bestBackCover, value); }
        //}
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
        IEnumerable<AcoustIDLookupResult> ILibraryLoaderImportOutput.AcoustIDResults
        {
            get
            {
                return _acoustIDResults.Select(x => new AcoustIDLookupResult()
                {
                    Fingerprint = x.Fingerprint,
                    LookupId = x.Id,
                    MusicBrainzRecordingId = x.MusicBrainzRecordingId,
                    Score = x.Score

                }).ToList();
            }
            set
            {
                _acoustIDResults.Clear();
                foreach (var result in value)
                {
                    _acoustIDResults.Add(new AcoustIDLookupResultViewModel()
                    {
                        Id = result.LookupId,
                        Fingerprint = result.Fingerprint,
                        MusicBrainzRecordingId = result.MusicBrainzRecordingId,
                        Score = result.Score
                    });
                }

                OnPropertyChanged("AcoustIDResults");
            }
        }
        IEnumerable<TagSmall> ILibraryLoaderImportOutput.MusicBrainzRecordingMatches
        {
            get
            {
                return _musicBrainzRecordingMatches.Select(x =>
                {
                    return new TagSmall()
                    {
                        Album = x.Album,
                        AlbumArtist = x.AlbumArtist,
                        MediaNumber = (int)x.MediaNumber,
                        MediaTotal = (int)x.MediaTotal,
                        MediaFormat = x.MediaFormat,
                        DurationMilliseconds = x.DurationMilliseconds,
                        Year = x.Year,
                        Genre = x.Genre,
                        Title = x.Title,
                        TrackNumber = (int)x.Track,
                        TrackTotal = (int)x.TrackTotal
                    };
                }).ToList();
            }
            set
            {
                _musicBrainzRecordingMatches.Clear();

                foreach (var result in value)
                {
                    var tagSmall = new TagSmallViewModel()
                    {
                        Album = result.Album ?? string.Empty,
                        AlbumArtist = result.AlbumArtist ?? string.Empty,
                        DurationMilliseconds = result.DurationMilliseconds ?? 0,
                        MediaNumber = result.MediaNumber ?? 0,
                        MediaTotal = result.MediaTotal ?? 0,
                        MediaFormat = result.MediaFormat ?? string.Empty,
                        Genre = result.Genre ?? string.Empty,
                        Title = result.Title ?? string.Empty,
                        Track = result.TrackNumber ?? 0,
                        TrackTotal = result.TrackTotal ?? 0
                    };

                    tagSmall.Validate();
                }

                OnPropertyChanged("MusicBrainzRecordingMatches");
            }
        }
        //IEnumerable<MusicBrainzCombinedLibraryEntryRecord> ILibraryLoaderImportOutput.MusicBrainzCombinedLibraryEntryRecords
        //{
        //    get { return _musicBrainzCombinedRecords; }
        //    set
        //    {
        //        _musicBrainzCombinedRecords.Clear();
        //        _musicBrainzCombinedRecords.AddRange(value);

        //        OnPropertyChanged("MusicBrainzCombinedLibraryEntryRecords");
        //    }
        //}
        #endregion

        public LibraryLoaderImportOutputViewModel()
        {
            this.AcoustIDResults = new ObservableCollection<AcoustIDLookupResultViewModel>();
            //this.MusicBrainzCombinedRecords = new ObservableCollection<MusicBrainzCombinedLibraryEntryRecord>();
            this.MusicBrainzRecordingMatches = new ObservableCollection<TagSmallViewModel>();
            this.LogMessages = new ObservableCollection<string>();
        }
    }
}
