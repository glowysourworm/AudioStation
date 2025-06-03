using System.Windows.Media;

using AudioStation.Component;
using AudioStation.Model.Database;
using AudioStation.Model.Vendor;
using AudioStation.ViewModel;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model
{
    public class LibraryEntry : ViewModelBase, IRecursiveSerializable
    {
        public event SimpleEventHandler<string, LogMessageSeverity> LogEvent;

        #region (private) Backing Fields
        SortedObservableCollection<Artist> _albumArtists;
        SortedObservableCollection<string> _genres;

        string _fileName;
        string _album;
        string _title;
        uint _year;
        uint _track;
        uint _disc;
        uint _discCount;
        TimeSpan _duration;

        // Music Brainz
        SortedObservableCollection<MusicBrainzRecord> _musicBrainzResults;            // Not Serialized (yet)
        MusicBrainzRecord _musicBrainzRecord;

        // Problems
        bool _fileMissing;
        bool _fileLoadError;
        bool _fileLocationNameMismatch;     // File folder path should be related to the album (see Tag loading)

        string _fileLoadErrorMessage;

        // Commands
        SimpleCommand _queryMusicBrainzCommand;
        #endregion

        #region (public) Tag Fields
        public string FileName
        {
            get { return _fileName; }
            private set { RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string PrimaryArtist
        {
            get { return _albumArtists.FirstOrDefault()?.Name ?? string.Empty; }
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
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }

        public SortedObservableCollection<Artist> AlbumArtists
        {
            get { return _albumArtists; }
            set { RaiseAndSetIfChanged(ref _albumArtists, value); }
        }
        public SortedObservableCollection<string> Genres
        {
            get { return _genres; }
            set { RaiseAndSetIfChanged(ref _genres, value); }
        }

        public bool FileMissing
        {
            get { return _fileMissing; }
            set { this.RaiseAndSetIfChanged(ref _fileMissing, value); }
        }
        public bool FileLoadError
        {
            get { return _fileLoadError; }
            set { this.RaiseAndSetIfChanged(ref _fileLoadError, value); }
        }
        public bool FileLocationNameMismatch
        {
            get { return _fileLocationNameMismatch; }
            set { this.RaiseAndSetIfChanged(ref _fileLocationNameMismatch, value); }
        }
        public string FileLoadErrorMessage
        {
            get { return _fileLoadErrorMessage; }
            set { this.RaiseAndSetIfChanged(ref _fileLoadErrorMessage, value); }
        }
        public SortedObservableCollection<MusicBrainzRecord> MusicBrainzResults
        {
            get { return _musicBrainzResults; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzResults, value); }
        }

        public MusicBrainzRecord MusicBrainzRecord
        {
            get { return _musicBrainzRecord; }
            set
            {
                this.RaiseAndSetIfChanged(ref _musicBrainzRecord, value);
                this.OnPropertyChanged("MusicBrainzRecordValid");
            }
        }
        public bool MusicBrainzRecordValid
        {
            get { return _musicBrainzRecord != MusicBrainzRecord.Empty; }
        }


        public SimpleCommand QueryMusicBrainzCommand
        {
            get { return _queryMusicBrainzCommand; }
            set { this.RaiseAndSetIfChanged(ref _queryMusicBrainzCommand, value); }
        }

        #endregion

        public LibraryEntry() { }

        public LibraryEntry(string file)
        {
            this.AlbumArtists = new SortedObservableCollection<Artist>();
            this.FileName = file;
            this.MusicBrainzRecord = MusicBrainzRecord.Empty;
            this.MusicBrainzResults = new SortedObservableCollection<MusicBrainzRecord>();

            this.MusicBrainzRecord.PropertyChanged += MusicBrainzRecord_PropertyChanged;

            this.OnPropertyChanged("MusicBrainzRecordValid");   // Calculated property (initialize)

            this.QueryMusicBrainzCommand = new SimpleCommand(async () =>
            {
                await QueryMusicBrainz();
            });
        }

        ~LibraryEntry()
        {
            this.MusicBrainzRecord.PropertyChanged -= MusicBrainzRecord_PropertyChanged;
        }

        private void MusicBrainzRecord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Dependent Properties
            this.OnPropertyChanged("MusicBrainzRecord");        // Bubble up property changes
            this.OnPropertyChanged("MusicBrainzRecordValid");   // Calculated property
        }

        private async Task QueryMusicBrainz()
        {
            try
            {
                OnLog("Querying MusicBrainz (remote): Title:  {0}", LogMessageSeverity.Info, this.Title);

                // Music Brainz!
                //
                var records = await MusicBrainzClient.Query(this);

                // TODO: Need Best Record Matcher
                this.MusicBrainzRecord = records.FirstOrDefault() ?? MusicBrainzRecord.Empty;

                foreach (var record in records)
                {
                    this.MusicBrainzResults.Add(record);
                }

                OnLog("MusicBrainz Query Finished: {0} Results (arranged by score)", LogMessageSeverity.Info, records.Count());
            }
            catch (Exception ex)
            {
                OnLog("Music Brainz Error:  {0}", LogMessageSeverity.Error, ex.Message);
            }
        }

        private void OnLog(string message, LogMessageSeverity severity, params object[] args)
        {
            if (this.LogEvent != null)
                this.LogEvent(string.Format(message, args), severity);
        }

        public LibraryEntry(IPropertyReader reader)
        {
            this.FileName = reader.Read<string>("FileName");

            this.Album = reader.Read<string>("Album");
            this.Title = reader.Read<string>("Title");
            this.Year = reader.Read<uint>("Year");
            this.Track = reader.Read<uint>("Track");
            this.Disc = reader.Read<uint>("Disc");
            this.DiscCount = reader.Read<uint>("DiscCount");
            this.AlbumArtists = reader.Read<SortedObservableCollection<Artist>>("AlbumArtists");
            this.Genres = reader.Read<SortedObservableCollection<string>>("Genres");

            this.FileMissing = reader.Read<bool>("FileMissing");
            this.FileLoadError = reader.Read<bool>("FileLoadError");
            this.FileLocationNameMismatch = reader.Read<bool>("FileLocationNameMismatch");
            this.FileLoadErrorMessage = reader.Read<string>("FileLoadErrorMessage");

            this.MusicBrainzRecord = reader.Read<MusicBrainzRecord>("MusicBrainzRecord");
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("FileName", this.FileName);

            writer.Write("Album", this.Album);
            writer.Write("Title", this.Title);
            writer.Write("Year", this.Year);
            writer.Write("Track", this.Track);
            writer.Write("Disc", this.Disc);
            writer.Write("DiscCount", this.DiscCount);
            writer.Write("AlbumArtists", this.AlbumArtists);
            writer.Write("Genres", this.Genres);

            writer.Write("FileMissing", this.FileMissing);
            writer.Write("FileLoadError", this.FileLoadError);
            writer.Write("FileLocationNameMismatch", this.FileLocationNameMismatch);
            writer.Write("FileLoadErrorMessage", this.FileLoadErrorMessage);

            writer.Write("MusicBrainzRecord", this.MusicBrainzRecord);
        }
    }
}
