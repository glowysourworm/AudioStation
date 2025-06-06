using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModels.Vendor
{
    public class MusicBrainzRecordViewModel : ViewModelBase
    {
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
            set { RaiseAndSetIfChanged(ref _score, value); }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { RaiseAndSetIfChanged(ref _timestamp, value); }
        }
        #endregion

        public MusicBrainzRecordViewModel(string recordingId)
        {
            this.MusicBrainzRecordingId = recordingId;
            this.Timestamp = DateTime.Now;
            this.AlbumArtists = new SortedObservableCollection<string>();
            this.Genres = new SortedObservableCollection<string>();
        }
    }
}
