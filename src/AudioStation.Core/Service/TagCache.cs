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
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IAudioConverter _audioConverter;

        SimpleDictionary<string, TagSmall> _tagSmallCache;
        SimpleDictionary<string, TagFull> _tagFullCache;

        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public TagCache(IAudioStationMapper audioStationMapper, IAudioConverter audioConverter)
        {
            _audioStationMapper = audioStationMapper;
            _audioConverter = audioConverter;
            _tagSmallCache = new SimpleDictionary<string, TagSmall>();
            _tagFullCache = new SimpleDictionary<string, TagFull>();
        }

        public TagSmall GetSmall(string fileName)
        {
            try
            {
                if (_tagSmallCache.ContainsKey(fileName))
                    return _tagSmallCache[fileName];

                Set(fileName);

                if (!_tagSmallCache.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _tagSmallCache[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public TagFull GetFull(string fileName)
        {
            try
            {
                if (_tagFullCache.ContainsKey(fileName))
                    return _tagFullCache[fileName];

                Set(fileName);

                if (!_tagFullCache.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _tagFullCache[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public TagSmall GetCopySmall(string fileName)
        {
            var tagSmall = GetSmall(fileName);

            return _audioStationMapper.Map<TagSmall, TagSmall>(tagSmall);
        }
        public TagFull GetCopyFull(string fileName)
        {
            var tagFull = GetFull(fileName);

            return _audioStationMapper.Map<TagFull, TagFull>(tagFull);
        }
        public void Set(string fileName)
        {
            if (_tagSmallCache.ContainsKey(fileName))
                _tagSmallCache.Remove(fileName);

            if (_tagFullCache.ContainsKey(fileName))
                _tagFullCache.Remove(fileName);

            // IdSharp -> AudioStation
            var tagFull = FromFile(fileName);

            // ITagFull -> ITagSmall
            var tagFileSmall = TagMapper.Map(tagFull);

            _tagSmallCache.Add(fileName, tagFileSmall);
            _tagFullCache.Add(fileName, tagFull);
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
                if (_tagSmallCache.ContainsKey(fileName))
                    _tagSmallCache.Remove(fileName);

                else
                    throw new Exception("Trying to evict tag file that was not yet cached! Please use this cache to get / set all tag files!");

                if (_tagFullCache.ContainsKey(fileName))
                    _tagFullCache.Remove(fileName);

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
            try
            {
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private TagFull FromFile(string fileName)
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

                        result.Images = tag.PictureList.Select(x => new LibraryImage(x.PictureType, x.MimeType, x.Description, x.PictureData)).ToList();

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
