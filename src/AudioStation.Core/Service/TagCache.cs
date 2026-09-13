using System.IO;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.IdSharp;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;

using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.RecursiveSerializer.Shared;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Service
{
    [IocExport(typeof(ITagCache))]
    public class TagCache : ITagCache
    {
        SimpleDictionary<string, TagSmall> _smallTags;
        SimpleDictionary<string, TagFull> _fullTags;                // Full tags are loaded from file

        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public TagCache()
        {
            _smallTags = new SimpleDictionary<string, TagSmall>();
            _fullTags = new SimpleDictionary<string, TagFull>();
        }

        public bool Verify(string fileName)
        {
            try
            {
                if (_fullTags.ContainsKey(fileName))
                    return true;

                Set(fileName);

                return _fullTags.ContainsKey(fileName);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Tag data invalid:  {0}", LogLevel.Warning, ex, fileName);
                return false;
            }
        }

        public TagFull Get(string fileName)
        {
            try
            {
                if (_fullTags.ContainsKey(fileName))
                    return _fullTags[fileName];

                Set(fileName);

                if (!_fullTags.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _fullTags[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public TagSmall GetSmall(string fileName)
        {
            try
            {
                if (_smallTags.ContainsKey(fileName))
                    return _smallTags[fileName];

                Set(fileName);

                if (!_smallTags.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _smallTags[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public TagFull GetCopy(string fileName)
        {
            // IdSharp -> AudioStation
            return FromFileFull(fileName);
        }
        public TagSmall GetCopySmall(string fileName)
        {
            var fullTag = GetCopy(fileName);

            return TagMapper.Shrink(fullTag);
        }
        public void Set(string fileName, bool fullTag = false)
        {
            // TODO: Use Full Tag option

            if (_fullTags.ContainsKey(fileName))
                _fullTags.Remove(fileName);

            if (_smallTags.ContainsKey(fileName))
                _smallTags.Remove(fileName);

            // IdSharp -> AudioStation
            var tagFile = FromFileFull(fileName);

            // ITagFull -> ITagSmall
            var tagFileSmall = TagMapper.Shrink(tagFile);

            _fullTags.Add(fileName, tagFile);
            _smallTags.Add(fileName, tagFileSmall);
        }

        public void SetData(string fileName, ITagFull tagData, bool save = true)
        {
            // Evict the cache before setting the data (no problem re-fetching)
            Evict(fileName);

            // Set directly to disk
            if (save)
                ToFile(fileName, tagData);
        }
        public void SetData(string fileName, ITagSmall tagData)
        {
            Evict(fileName);

            // Probably don't need to save this data. The next get will re-fill the
            // tag cache for both small and full tags
        }
        public void Evict(string fileName)
        {
            try
            {
                if (_fullTags.ContainsKey(fileName))
                    _fullTags.Remove(fileName);

                else
                    throw new Exception("Trying to evict tag file that was not yet cached! Please use this cache to get / set all tag files!");

                if (_smallTags.ContainsKey(fileName))
                    _smallTags.Remove(fileName);

                else
                    throw new Exception("Trying to evict tag file that was not yet cached! Please use this cache to get / set all tag files!");
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error evicting tag data:  {0}", LogLevel.Error, ex, fileName);
                throw ex;
            }
        }
        public byte[] Serialize(AudioStationTag serializableTag)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new RecursiveSerializer<AudioStationTag>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                serializer.Serialize(stream, serializableTag);

                return stream.GetBuffer();
            }
        }
        public TagFull Deserialize(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var serializer = new RecursiveSerializer<TagFull>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                stream.Seek(0, SeekOrigin.Begin);

                return serializer.Deserialize(stream);
            }
        }

        private void ToFile(string fileName, ITagFull tag)
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

        private TagFull FromFileFull(string fileName)
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
