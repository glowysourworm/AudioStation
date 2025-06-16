using System.ComponentModel;

using AutoMapper;

using SimpleWpf.ObjectMapping;

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

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        TagTypes _tagTypes;

        public override TagTypes TagTypes
        {
            get { return _tagTypes; }
        }
        string _album;

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
        string[] _albumArtists;

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
        string[] _albumArtistsSort;

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
        string _title;

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
        string _titleSort;

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
        string[] _performers;

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
        string[] _performersSort;

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
        string[] _composers;

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
        string[] _composersSort;

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
        string _albumSort;

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
        string _comment;

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
        string[] _genres;

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
        uint _year;

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
        uint _track;

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
        uint _trackCount;

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
        uint _disc;

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
        uint _discCount;

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
        string _lyrics;

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
        string _grouping;

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
        uint _beatsPerMinute;

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
        string _conductor;

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
        string _copyright;

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
        string _muiscBrainzArtistId;

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
        string _musicBrainzReleaseId;

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
        string _musicBrainzReleaseArtistId;

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
        string _musicBrainzTrackId;

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
        string _musicBrainzDiscId;

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
        string _musicIpId;

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
        string _amazonId;

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
        string _musicBrainzReleaseStatus;

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
        string _musicBrainzReleaseType;

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
        string _musicBrainzReleaseCountry;

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
        double _replayGainTrackGain;

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
        double _replayGainTrackPeak;

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
        double _replayGainAlbumGain;

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
        double _replayGainAlbumPeak;

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
        IPicture[] _pictures;

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
        string[] _artists;

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
        bool _isEmpty;

        public override bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public TagViewModel()
        {
        }
        public TagViewModel(TagLib.Tag tag)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TagLib.Tag, TagViewModel>(MemberList.Destination));
            var mapper = config.CreateMapper();

            mapper.Map(tag, this, typeof(TagLib.Tag), typeof(TagViewModel));

            _tagTypes = tag.TagTypes;
        }
    }
}
