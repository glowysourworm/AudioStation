using System.ComponentModel;

using AudioStation.Core.Utility;

using AutoMapper;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    /// <summary>
    /// TagLib (shared) view model:  It appears that the Tag (abstract) base class is a shared
    /// view model that has data allocations for each / all tag types. All of the virtual and
    /// abstract properties must be overridden to build our view model.
    /// </summary>
    public class TagViewModel : TagLib.Tag, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        TagTypes _tagTypes;
        string _album;
        string[] _albumArtists;
        string[] _albumArtistsSort;
        string _title;
        string _titleSort;
        string[] _performers;
        string[] _performersSort;
        string[] _composers;
        string[] _composersSort;
        string _albumSort;
        string _comment;
        string[] _genres;
        uint _year;
        uint _track;
        uint _trackCount;
        uint _disc;
        uint _discCount;
        string _lyrics;
        string _grouping;
        uint _beatsPerMinute;
        string _conductor;
        string _copyright;
        string _muiscBrainzArtistId;
        string _musicBrainzReleaseId;
        string _musicBrainzReleaseArtistId;
        string _musicBrainzTrackId;
        string _musicBrainzDiscId;
        string _musicIpId;
        string _amazonId;
        string _musicBrainzReleaseStatus;
        string _musicBrainzReleaseType;
        string _musicBrainzReleaseCountry;
        double _replayGainTrackGain;
        double _replayGainTrackPeak;
        double _replayGainAlbumGain;
        double _replayGainAlbumPeak;
        IPicture[] _pictures;
        string[] _artists;
        bool _isEmpty;

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override TagTypes TagTypes
        {
            get { return _tagTypes; }
        }
        public override string Album
        {
            get { return _album; }
            set
            {
                _album = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Album"));
            }
        }
        public override string[] AlbumArtists
        {
            get { return _albumArtists; }
            set
            {
                _albumArtists = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AlbumArtists"));
            }
        }
        public override string[] AlbumArtistsSort
        {
            get { return _albumArtistsSort; }
            set
            {
                _albumArtistsSort = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AlbumArtistsSort"));
            }
        }
        public override string Title
        {
            get { return _title; }
            set
            {
                _title = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Title"));
            }
        }
        public override string TitleSort
        {
            get { return _titleSort; }
            set
            {
                _titleSort = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("TitleSort"));
            }
        }
        public override string[] Performers
        {
            get { return _performers; }
            set
            {
                _performers = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Performers"));
            }
        }
        public override string[] PerformersSort
        {
            get { return _performersSort; }
            set
            {
                _performersSort = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("PerformersSort"));
            }
        }
        public override string[] Composers
        {
            get { return _composers; }
            set
            {
                _composers = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Composers"));
            }
        }
        public override string[] ComposersSort
        {
            get { return _composersSort; }
            set
            {
                _composersSort = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ComposersSort"));
            }
        }
        public override string AlbumSort
        {
            get { return _albumSort; }
            set
            {
                _albumSort = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AlbumSort"));
            }
        }
        public override string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Comment"));
            }
        }
        public override string[] Genres
        {
            get { return _genres; }
            set
            {
                _genres = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Genres"));
            }
        }
        public override uint Year
        {
            get { return _year; }
            set
            {
                _year = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Year"));
            }
        }
        public override uint Track
        {
            get { return _track; }
            set
            {
                _track = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Track"));
            }
        }
        public override uint TrackCount
        {
            get { return _trackCount; }
            set
            {
                _trackCount = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("TrackCount"));
            }
        }
        public override uint Disc
        {
            get { return _disc; }
            set
            {
                _disc = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Disc"));
            }
        }
        public override uint DiscCount
        {
            get { return _discCount; }
            set
            {
                _discCount = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("DiscCount"));
            }
        }
        public override string Lyrics
        {
            get { return _lyrics; }
            set
            {
                _lyrics = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Lyrics"));
            }
        }
        public override string Grouping
        {
            get { return _grouping; }
            set
            {
                _grouping = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Grouping"));
            }
        }
        public override uint BeatsPerMinute
        {
            get { return _beatsPerMinute; }
            set
            {
                _beatsPerMinute = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("BeatsPerMinute"));
            }
        }
        public override string Conductor
        {
            get { return _conductor; }
            set
            {
                _conductor = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Conductor"));
            }
        }
        public override string Copyright
        {
            get { return _copyright; }
            set
            {
                _copyright = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Copyright"));
            }
        }
        public override string MusicBrainzArtistId
        {
            get { return _muiscBrainzArtistId; }
            set
            {
                _muiscBrainzArtistId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzArtistId"));
            }
        }
        public override string MusicBrainzReleaseId
        {
            get { return _musicBrainzReleaseId; }
            set
            {
                _musicBrainzReleaseId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzReleaseId"));
            }
        }
        public override string MusicBrainzReleaseArtistId
        {
            get { return _musicBrainzReleaseArtistId; }
            set
            {
                _musicBrainzReleaseArtistId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzReleaseArtistId"));
            }
        }
        public override string MusicBrainzTrackId
        {
            get { return _musicBrainzTrackId; }
            set
            {
                _musicBrainzTrackId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzTrackId"));
            }
        }
        public override string MusicBrainzDiscId
        {
            get { return _musicBrainzDiscId; }
            set
            {
                _musicBrainzDiscId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzDiscId"));
            }
        }
        public override string MusicIpId
        {
            get { return _musicIpId; }
            set
            {
                _musicIpId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicIpId"));
            }
        }
        public override string AmazonId
        {
            get { return _amazonId; }
            set
            {
                _amazonId = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AmazonId"));
            }
        }
        public override string MusicBrainzReleaseStatus
        {
            get { return _musicBrainzReleaseStatus; }
            set
            {
                _musicBrainzReleaseStatus = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzReleaseStatus"));
            }
        }
        public override string MusicBrainzReleaseType
        {
            get { return _musicBrainzReleaseType; }
            set
            {
                _musicBrainzReleaseType = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzReleaseType"));
            }
        }
        public override string MusicBrainzReleaseCountry
        {
            get { return _musicBrainzReleaseCountry; }
            set
            {
                _musicBrainzReleaseCountry = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("MusicBrainzReleaseCountry"));
            }
        }
        public override double ReplayGainTrackGain
        {
            get { return _replayGainTrackGain; }
            set
            {
                _replayGainTrackGain = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ReplayGainTrackGain"));
            }
        }
        public override double ReplayGainTrackPeak
        {
            get { return _replayGainTrackPeak; }
            set
            {
                _replayGainTrackPeak = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ReplayGainTrackPeak"));
            }
        }
        public override double ReplayGainAlbumGain
        {
            get { return _replayGainAlbumGain; }
            set
            {
                _replayGainAlbumGain = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ReplayGainAlbumGain"));
            }
        }
        public override double ReplayGainAlbumPeak
        {
            get { return _replayGainAlbumPeak; }
            set
            {
                _replayGainAlbumPeak = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ReplayGainAlbumPeak"));
            }
        }
        public override IPicture[] Pictures
        {
            get { return _pictures; }
            set
            {
                _pictures = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Pictures"));
            }
        }
        public override string[] Artists
        {
            get { return _artists; }
            set
            {
                _artists = value;

                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Artists"));
            }
        }
        public override bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public TagViewModel()
        {
            this.Title = string.Empty;
            this.TitleSort = string.Empty;
            this.AlbumSort = string.Empty;
            this.Comment = string.Empty;
            this.Lyrics = string.Empty;
            this.Grouping = string.Empty;
        }
        public TagViewModel(TagLib.Tag tag)
        {
            ApplicationHelpers.MapOnto(tag, this);

            _tagTypes = tag.TagTypes;
        }
    }
}
