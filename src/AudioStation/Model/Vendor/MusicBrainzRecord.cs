using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Model.Vendor
{
    /// <summary>
    /// Extracted data from music brainz query
    /// </summary>
    public class MusicBrainzRecord : ViewModelBase
    {
        public static MusicBrainzRecord Empty;

        #region (private) Backing Fields
        SortedObservableCollection<string> _albumArtists;
        SortedObservableCollection<string> _genres;

        string _musicBrainzRecordingId;
        string _musicBrainzReleaseCountry;
        string _musicBrainzReleaseStatus;

        string _album;
        string _title;
        uint _year;
        uint _track;
        uint _disc;
        uint _discCount;

        int _score;
        DateTime _timestamp;
        #endregion

        #region (public) Tag + MusicBrainz Fields
        public string MusicBrainzRecordingId
        {
            get { return _musicBrainzRecordingId; }
            private set { RaiseAndSetIfChanged(ref _musicBrainzRecordingId, value); }
        }
        public string MusicBrainzReleaseCountry
        {
            get { return _musicBrainzReleaseCountry; }
            set { RaiseAndSetIfChanged(ref _musicBrainzReleaseCountry, value); }
        }
        public string MusicBrainzReleaseStatus
        {
            get { return _musicBrainzReleaseStatus; }
            set { RaiseAndSetIfChanged(ref _musicBrainzReleaseStatus, value); }
        }
        public string Album
        {
            get { return _album; }
            set { RaiseAndSetIfChanged(ref _album, value); }
        }
        public string Title
        {
            get { return _title; }
            set { RaiseAndSetIfChanged(ref _title, value); }
        }
        public uint Year
        {
            get { return _year; }
            set { RaiseAndSetIfChanged(ref _year, value); }
        }
        public uint Track
        {
            get { return _track; }
            set { RaiseAndSetIfChanged(ref _track, value); }
        }
        public uint Disc
        {
            get { return _disc; }
            set { RaiseAndSetIfChanged(ref _disc, value); }
        }
        public uint DiscCount
        {
            get { return _discCount; }
            set { RaiseAndSetIfChanged(ref _discCount, value); }
        }

        public SortedObservableCollection<string> AlbumArtists
        {
            get { return _albumArtists; }
            set { RaiseAndSetIfChanged(ref _albumArtists, value); }
        }
        public SortedObservableCollection<string> Genres
        {
            get { return _genres; }
            set { RaiseAndSetIfChanged(ref _genres, value); }
        }
        public int Score
        {
            get { return _score; }
            set { this.RaiseAndSetIfChanged(ref _score, value); }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { this.RaiseAndSetIfChanged(ref _timestamp, value); }
        }
        public bool IsValid
        {
            get { return this != MusicBrainzRecord.Empty; }
        }
        #endregion

        static MusicBrainzRecord()
        {
            Empty = new MusicBrainzRecord("Empty (not queried)");
        }
        public MusicBrainzRecord(string recordingId)
        {
            this.MusicBrainzRecordingId = recordingId;
            this.Timestamp = DateTime.Now;
            this.AlbumArtists = new SortedObservableCollection<string>();
            this.Genres = new SortedObservableCollection<string>();
        }
    }
}
