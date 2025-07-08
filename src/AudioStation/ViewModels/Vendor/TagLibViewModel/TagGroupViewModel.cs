using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Metadata.Internal;

using SimpleWpf.Extensions.Collection;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class TagGroupViewModel : TagLib.Tag, IList<TagViewModel>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public enum GroupType
        {
            None = 0,
            Artist = 1,
            ArtistAlbum = 2
        }

        public int Count { get { return _tags.Count; } }
        public bool IsReadOnly { get { return false; } }

        public TagViewModel this[int index]
        {
            get { return _tags[index]; }
            set { _tags[index] = value; }
        }

        GroupType _groupType;
        List<TagViewModel> _tags;

        public TagGroupViewModel()
            : this(GroupType.None, Enumerable.Empty<TagViewModel>())
        { }
        public TagGroupViewModel(GroupType groupType, IEnumerable<TagViewModel> tags)
        {
            _groupType = groupType;
            _tags = new List<TagViewModel>(tags);
        }

        public override TagTypes TagTypes
        {
            get { return GetProperty(x => x.TagTypes); }
        }
        public override string Album
        {
            get { return GetProperty(x => x.Album, string.Empty); }
            set { SetProperty(value, x => x.Album); }
        }
        public override string[] AlbumArtists
        {
            get { return GetProperty(x => x.AlbumArtists, new string[] { }); }
            set { SetProperty(value, x => x.AlbumArtists); }
        }
        public override string[] AlbumArtistsSort
        {
            get { return GetProperty(x => x.AlbumArtistsSort, new string[] { }); }
            set { SetProperty(value, x => x.AlbumArtistsSort); }
        }
        public override string Title
        {
            get { return GetProperty(x => x.Title, string.Empty); }
            set { SetProperty(value, x => x.Title); }
        }
        public override string TitleSort
        {
            get { return GetProperty(x => x.TitleSort, string.Empty); }
            set { SetProperty(value, x => x.TitleSort); }
        }
        public override string[] Performers
        {
            get { return GetProperty(x => x.Performers, new string[] { }); }
            set { SetProperty(value, x => x.Performers); }
        }
        public override string[] PerformersSort
        {
            get { return GetProperty(x => x.PerformersSort, new string[] { }); }
            set { SetProperty(value, x => x.PerformersSort); }
        }
        public override string[] Composers
        {
            get { return GetProperty(x => x.Composers, new string[] { }); }
            set { SetProperty(value, x => x.Composers); }
        }
        public override string[] ComposersSort
        {
            get { return GetProperty(x => x.ComposersSort, new string[] { }); }
            set { SetProperty(value, x => x.ComposersSort); }
        }
        public override string AlbumSort
        {
            get { return GetProperty(x => x.AlbumSort, string.Empty); }
            set { SetProperty(value, x => x.AlbumSort); }
        }
        public override string Comment
        {
            get { return GetProperty(x => x.Comment, string.Empty); }
            set { SetProperty(value, x => x.Comment); }
        }
        public override string[] Genres
        {
            get { return GetProperty(x => x.Genres, new string[] { }); }
            set { SetProperty(value, x => x.Genres); }
        }
        public override uint Year
        {
            get { return GetProperty(x => x.Year); }
            set { SetProperty(value, x => x.Year); }
        }
        public override uint Track
        {
            get { return GetProperty(x => x.Track); }
            set { SetProperty(value, x => x.Track); }
        }
        public override uint TrackCount
        {
            get { return GetProperty(x => x.TrackCount); }
            set { SetProperty(value, x => x.TrackCount); }
        }
        public override uint Disc
        {
            get { return GetProperty(x => x.Disc); }
            set { SetProperty(value, x => x.Disc); }
        }
        public override uint DiscCount
        {
            get { return GetProperty(x => x.DiscCount); }
            set { SetProperty(value, x => x.DiscCount); }
        }
        public override string Lyrics
        {
            get { return GetProperty(x => x.Lyrics, string.Empty); }
            set { SetProperty(value, x => x.Lyrics); }
        }
        public override string Grouping
        {
            get { return GetProperty(x => x.Grouping, string.Empty); }
            set { SetProperty(value, x => x.Grouping); }
        }
        public override uint BeatsPerMinute
        {
            get { return GetProperty(x => x.BeatsPerMinute); }
            set { SetProperty(value, x => x.BeatsPerMinute); }
        }
        public override string Conductor
        {
            get { return GetProperty(x => x.Conductor, string.Empty); }
            set { SetProperty(value, x => x.Conductor); }
        }
        public override string Copyright
        {
            get { return GetProperty(x => x.Copyright, string.Empty); }
            set { SetProperty(value, x => x.Copyright); }
        }
        public override string MusicBrainzArtistId
        {
            get { return GetProperty(x => x.MusicBrainzArtistId, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzArtistId); }
        }
        public override string MusicBrainzReleaseId
        {
            get { return GetProperty(x => x.MusicBrainzReleaseId, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzReleaseId); }
        }
        public override string MusicBrainzReleaseArtistId
        {
            get { return GetProperty(x => x.MusicBrainzReleaseArtistId, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzReleaseArtistId); }
        }
        public override string MusicBrainzTrackId
        {
            get { return GetProperty(x => x.MusicBrainzTrackId, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzTrackId); }
        }
        public override string MusicBrainzDiscId
        {
            get { return GetProperty(x => x.MusicBrainzDiscId, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzDiscId); }
        }
        public override string MusicIpId
        {
            get { return GetProperty(x => x.MusicIpId, string.Empty); }
            set { SetProperty(value, x => x.MusicIpId); }
        }
        public override string AmazonId
        {
            get { return GetProperty(x => x.AmazonId, string.Empty); }
            set { SetProperty(value, x => x.AmazonId); }
        }
        public override string MusicBrainzReleaseStatus
        {
            get { return GetProperty(x => x.MusicBrainzReleaseStatus, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzReleaseStatus); }
        }
        public override string MusicBrainzReleaseType
        {
            get { return GetProperty(x => x.MusicBrainzReleaseType, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzReleaseType); }
        }
        public override string MusicBrainzReleaseCountry
        {
            get { return GetProperty(x => x.MusicBrainzReleaseCountry, string.Empty); }
            set { SetProperty(value, x => x.MusicBrainzReleaseCountry); }
        }
        public override double ReplayGainTrackGain
        {
            get { return GetProperty(x => x.ReplayGainTrackGain); }
            set { SetProperty(value, x => x.ReplayGainTrackGain); }
        }
        public override double ReplayGainTrackPeak
        {
            get { return GetProperty(x => x.ReplayGainTrackPeak); }
            set { SetProperty(value, x => x.ReplayGainTrackPeak); }
        }
        public override double ReplayGainAlbumGain
        {
            get { return GetProperty(x => x.ReplayGainAlbumGain); }
            set { SetProperty(value, x => x.ReplayGainAlbumGain); }
        }
        public override double ReplayGainAlbumPeak
        {
            get { return GetProperty(x => x.ReplayGainAlbumPeak); }
            set { SetProperty(value, x => x.ReplayGainAlbumPeak); }
        }
        public override IPicture[] Pictures
        {
            get { return GetProperty(x => x.Pictures, new IPicture[] { }); }
            set { SetProperty(value, x => x.Pictures); }
        }
        public override string[] Artists
        {
            get { return GetProperty(x => x.Artists, new string[] { }); }
            set { SetProperty(value, x => x.Artists); }
        }
        public override bool IsEmpty
        {
            get { return GetProperty(x => x.IsEmpty); }
        }

        protected T GetProperty<T>(Func<TagViewModel, T> propertyExpression, T defaultValue = default(T))
        {
            return _tags.GetGroupProperty(propertyExpression, defaultValue);
        }

        protected void SetProperty<T>(T propertyValue, Expression<Func<TagViewModel, T>> propertyExpression)
        {
            _tags.SetGroupProperty(propertyValue, propertyExpression);

            OnPropertyChanged(propertyExpression);
        }

        protected void OnPropertyChanged<T>(Expression<Func<TagViewModel, T>> propertyExpression)
        {
            var lambda = propertyExpression.Body as MemberExpression;

            if (lambda != null &&
                lambda.NodeType == ExpressionType.MemberAccess)
            {              
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(lambda.Member.Name));
            }
        }

        #region IList Methods
        public override void Clear()
        {
            _tags.Clear();
        }
        public int IndexOf(TagViewModel item)
        {
            return _tags.IndexOf(item);
        }
        public void Insert(int index, TagViewModel item)
        {
            _tags.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            _tags.RemoveAt(index);
        }
        public void Add(TagViewModel item)
        {
            _tags.Add(item);
        }
        public bool Contains(TagViewModel item)
        {
            return _tags.Contains(item);
        }
        public void CopyTo(TagViewModel[] array, int arrayIndex)
        {
            _tags.CopyTo(array, arrayIndex);
        }
        public bool Remove(TagViewModel item)
        {
            return _tags.Remove(item);
        }
        public IEnumerator<TagViewModel> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
