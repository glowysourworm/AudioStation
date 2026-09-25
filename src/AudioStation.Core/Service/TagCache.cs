using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.IdSharp;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;

using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Service
{
    [IocExport(typeof(ITagCache))]
    public class TagCache : ITagCache
    {
        private readonly IAudioConverter _audioConverter;

        SimpleDictionary<string, TagSmall> _tags;

        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public TagCache(IAudioConverter audioConverter)
        {
            _audioConverter = audioConverter;
            _tags = new SimpleDictionary<string, TagSmall>();
        }

        public TagSmall Get(string fileName)
        {
            try
            {
                if (_tags.ContainsKey(fileName))
                    return _tags[fileName];

                Set(fileName);

                if (!_tags.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _tags[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public TagSmall GetCopy(string fileName)
        {
            var duration = TimeSpan.Zero;
            var fullTag = FromFileFull(fileName, out duration);

            return TagMapper.Map(fullTag);
        }
        public TagFull GetFullTag(string fileName, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;
            return FromFileFull(fileName, out duration);
        }
        public void Set(string fileName)
        {
            if (_tags.ContainsKey(fileName))
                _tags.Remove(fileName);

            // IdSharp -> AudioStation
            var duration = TimeSpan.Zero;
            var tagFile = FromFileFull(fileName, out duration);

            // ITagFull -> ITagSmall
            var tagFileSmall = TagMapper.Map(tagFile);

            _tags.Add(fileName, tagFileSmall);
        }

        public void SetData(string fileName, ITagSmall tagData, bool save = true)
        {
            // Evict the cache before setting the data (no problem re-fetching)
            Evict(fileName);

            // Set directly to disk
            if (save)
                ToFile(fileName, tagData);
        }
        public void Evict(string fileName)
        {
            try
            {
                if (_tags.ContainsKey(fileName))
                    _tags.Remove(fileName);

                else
                    throw new Exception("Trying to evict tag file that was not yet cached! Please use this cache to get / set all tag files!");
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error evicting tag data:  {0}", LogLevel.Error, ex, fileName);
                throw ex;
            }
        }

        private void ToFile(string fileName, ITagSmall tag)
        {
            throw new NotImplementedException();

            try
            {
                // AudioStation -> ATL:  Some properties are ready only. All fields have been put in their proper place by
                //                       this point. (see IAudioStationTag)
                //
                //var atlTrack = TagMapper.MapTo(tag, fileName);

                // ATL:  Save (to ATL.Track.Path = fileName)
                //atlTrack.Save();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private TagFull FromFileFull(string fileName, out TimeSpan duration)
        {
            try
            {
                // CSCore
                //var id3v2 = ID3v2.FromFile(fileName);

                using (var fileStream = new FileStream(fileName, new FileStreamOptions()
                {
                    Access = FileAccess.Read,
                    Mode = FileMode.Open,
                    Options = FileOptions.SequentialScan
                }))
                {
                    // TODO: Decide how to handle duration
                    //
                    //var durationMilliseconds = _audioConverter.GetDurationMilliseconds(fileStream);
                    //duration = TimeSpan.FromMilliseconds(durationMilliseconds);

                    duration = TimeSpan.Zero;

                    // Reset Stream
                    fileStream.Position = 0;

                    TagFull result = new TagFull();

                    var isId3v1 = ID3v1Tag.DoesTagExist(fileStream);
                    var isId3v2 = ID3v2Tag.DoesTagExist(fileStream);

                    if (isId3v1)
                    {
                        var tag = new ID3v1Tag(fileStream);

                        result.Album = tag.Album;
                        result.Artist = tag.Artist;
                        result.Genre = GenreHelper.GetSortedGenreList()[tag.GenreIndex];
                        result.Title = tag.Title;
                        result.TrackNumber = tag.TrackNumber;

                        var year = 0;
                        if (int.TryParse(tag.Year, out year))
                            result.Year = year;
                    }

                    // MUST RESET POSITION (before letting IdSharp read its next tag)
                    fileStream.Position = 0;

                    if (isId3v2)
                    {
                        var tag = new ID3v2Tag(fileStream);

                        result.Album = tag.Album;
                        result.AlbumArtist = tag.AlbumArtist;
                        result.Artist = tag.Artist;
                        result.Comment = tag.CommentsList.Join(";", x => x.Value);
                        result.Copyright = tag.Copyright;
                        result.DurationMilliseconds = tag.LengthMilliseconds;
                        result.Genre = tag.Genre;

                        foreach (var person in tag.InvolvedPersonList.Items)
                        {
                            result.InvolvedPeople.Add(new InvolvedPerson()
                            {
                                Involvement = person.Involvement,
                                Name = person.Name
                            });
                        }

                        result.MediaFormat = tag.MediaType;

                        var mediaNumber = 0;
                        var mediaTotal = 0;
                        var trackNumber = 0;
                        var trackTotal = 0;

                        if (!string.IsNullOrWhiteSpace(tag.DiscNumber))
                        {
                            // Format:  (#/#)
                            if (TagHelpers.ID3V2Parse_TRK_or_TPOS(tag.DiscNumber, out mediaNumber, out mediaTotal))
                            {
                                result.MediaNumber = mediaNumber;
                                result.MediaTotal = mediaTotal;
                            }

                            // Format: (#) (try just a single number) (this will be fixed on save)
                            else if (int.TryParse(tag.DiscNumber, out mediaNumber))
                            {
                                result.MediaNumber = mediaNumber;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(tag.TrackNumber))
                        {
                            // Format:  (#/#)
                            if (TagHelpers.ID3V2Parse_TRK_or_TPOS(tag.TrackNumber, out trackNumber, out trackTotal))
                            {
                                result.TrackNumber = trackNumber;
                                result.TrackTotal = trackTotal;
                            }

                            // Format: (#) (try just a single number) (this will be fixed on save)
                            else if (int.TryParse(tag.TrackNumber, out trackNumber))
                            {
                                result.TrackNumber = trackNumber;
                            }
                        }

                        result.Publisher = tag.Publisher;
                        result.SortAlbum = tag.AlbumSortOrder;
                        result.SortAlbumArtist = tag.ArtistSortOrder;
                        result.SortArtist = tag.ArtistSortOrder;
                        result.SortTitle = tag.TitleSortOrder;
                        result.Title = tag.Title;

                        foreach (var userField in tag.UserDefinedText)
                        {
                            result.UserDefinedFields.Add(new UserDefinedField()
                            {
                                Description = userField.Description,
                                Value = userField.Value
                            });
                        }

                        foreach (var identifier in tag.UniqueFileIdentifierList)
                        {
                            result.UniqueFileIdentifiers.Add(new UniqueFileIdentifier()
                            {
                                Identifier = identifier.Identifier,
                                OwnerIdentifier = identifier.OwnerIdentifier
                            });
                        }

                        var year = 0;
                        if (int.TryParse(tag.Year, out year))
                            result.Year = year;
                    }

                    fileStream.Flush();
                    fileStream.Close();

                    return result;
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error reading tag data (rebuilding Mp3 file):  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
