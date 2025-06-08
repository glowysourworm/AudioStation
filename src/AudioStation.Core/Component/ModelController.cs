using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.Model;

using m3uParser.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

using TagLib;

namespace AudioStation.Controller
{
    [IocExport(typeof(IModelController))]
    public class ModelController : IModelController
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;

        public Library Library { get; private set; }
        public Radio Radio { get; private set; }

        [IocImportingConstructor]
        public ModelController(IConfigurationManager configurationManager, IOutputController outputController)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
        }

        public void Initialize()
        {
            // Procedure: The Library and Radio components are loaded from
            //            any database data that exists. At runtime, the rest
            //            of the data may be queried from this controller.
            //
            this.Library = new Library();
            this.Radio = new Radio();

            try
            {
                using (var context = CreateContext())
                {
                    // Load data from the database that will be used to group / view entries. The
                    // rest of the data will be loaded lazily. The mp3 tag data will be called once
                    // the view loads the Album, Artist, or Entry (views). So, don't discard the 
                    // data. Flag the Library (local entites) that they have been read / not-read.

                    var fileRefs = context.Mp3FileReferences.Select(x => new LibraryEntry()
                    {
                        Album = x.Album.Name,
                        FileName = x.FileName,
                        PrimaryArtist = x.PrimaryArtist.Name,
                        Title = x.Title,
                        Track = (uint)x.Track,
                        Disc = (uint)x.Album.DiscNumber,
                        PrimaryGenre = context.Mp3FileReferenceGenreMaps.First(z => z.Mp3FileReferenceId == x.Id).Mp3FileReferenceGenre.Name,
                        Id = x.Id

                    }).ToList();
                    var albums = context.Mp3FileReferenceAlbums.Select(x => new Album()
                    {
                        Name = x.Name

                    }).ToList();
                    var artists = context.Mp3FileReferenceArtists.Select(x => new Artist()
                    {
                        Name = x.Name,

                    }).ToList();

                    var genres = context.Mp3FileReferenceGenres.Select(x => x.Name).ToList();

                    // Aggregated data
                    foreach (var album in albums)
                    {
                        album.Tracks = fileRefs.Where(x => x.Album == album.Name).ToList();
                    }
                    foreach (var artist in artists)
                    {
                        artist.Albums = albums.Where(album => album.Tracks.All(track => track.PrimaryArtist == artist.Name)).ToList();
                    }

                    this.Library.Albums = albums;
                    this.Library.Artists = artists;
                    this.Library.Genres = genres;
                    this.Library.Titles = fileRefs;
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error initializing IModelController:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public void AddUpdateLibraryEntry(string fileName, TagLib.File tagRef)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var entity = context.Mp3FileReferences.FirstOrDefault(x => x.FileName == fileName);
                    var newEntity = false;

                    if (entity == null)
                    {
                        entity = new Mp3FileReference()
                        {
                            FileName = fileName,
                            Title = tagRef.Tag.Title?.Trim() ?? string.Empty,
                            Track = (int)tagRef.Tag.Track,
                        };
                        newEntity = true;
                    }

                    // There could be Null / Empty / or Unknown data. Assume there is.
                    var existingAlbum = context.Mp3FileReferenceAlbums.FirstOrDefault(x => x.Name == tagRef.Tag.Album);
                    var existingArtist = context.Mp3FileReferenceArtists.FirstOrDefault(x => x.Name == tagRef.Tag.FirstAlbumArtist);

                    // Just check for null or white space
                    if (existingAlbum == null && !string.IsNullOrWhiteSpace(tagRef.Tag.Album))
                    {
                        existingAlbum = new Mp3FileReferenceAlbum()
                        {
                            DiscCount = (int)tagRef.Tag.DiscCount,
                            DiscNumber = (int)tagRef.Tag.Disc,
                            Name = tagRef.Tag.Album.Trim()
                        };

                        context.Mp3FileReferenceAlbums.Add(existingAlbum);
                    }
                    if (existingArtist == null && !string.IsNullOrWhiteSpace(tagRef.Tag.FirstAlbumArtist))
                    {
                        existingArtist = new Mp3FileReferenceArtist()
                        {
                            Name = tagRef.Tag.FirstAlbumArtist.Trim()
                        };

                        context.Mp3FileReferenceArtists.Add(existingArtist);
                    }

                    entity.PrimaryArtist = existingArtist;
                    entity.Album = existingAlbum;

                    if (newEntity)
                        context.Add(entity);
                    else
                        context.Update(entity);

                    context.SaveChanges();

                    // Add Maps
                    var lastEntity = context.Mp3FileReferences.First(x => x.FileName == fileName);

                    // Artist Map(s)
                    foreach (var artist in tagRef.Tag.AlbumArtists.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        var artistEntity = context.Mp3FileReferenceArtists
                                                  .FirstOrDefault(x => x.Name == artist);

                        var map = context.Mp3FileReferenceArtistMaps
                                         .FirstOrDefault(x => x.Mp3FileReferenceArtist.Name == artist && x.Mp3FileReferenceId == lastEntity.Id);

                        // New Genre
                        if (artistEntity == null)
                        {
                            artistEntity = new Mp3FileReferenceArtist()
                            {
                                Name = artist
                            };
                            context.Mp3FileReferenceArtists.Add(artistEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new Mp3FileReferenceArtistMap()
                            {
                                Mp3FileReference = lastEntity,
                                Mp3FileReferenceArtist = artistEntity
                            };
                            context.Mp3FileReferenceArtistMaps.Add(map);
                        }
                    }

                    // Genre Map(s)
                    foreach (var genre in tagRef.Tag.Genres.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        var genreEntity = context.Mp3FileReferenceGenres
                                                 .FirstOrDefault(x => x.Name == genre);

                        var map = context.Mp3FileReferenceGenreMaps
                                         .FirstOrDefault(x => x.Mp3FileReferenceGenre.Name == genre && x.Mp3FileReferenceId == lastEntity.Id);

                        // New Genre
                        if (genreEntity == null)
                        {
                            genreEntity = new Mp3FileReferenceGenre()
                            {
                                Name = genre
                            };
                            context.Mp3FileReferenceGenres.Add(genreEntity);
                        }

                        // New Map
                        if (map == null)
                        {
                            map = new Mp3FileReferenceGenreMap()
                            {
                                Mp3FileReference = lastEntity,
                                Mp3FileReferenceGenre = genreEntity                                
                            };
                            context.Mp3FileReferenceGenreMaps.Add(map);
                        }
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public void AddRadioEntry(Extm3u entry)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var entity = new M3UInfo()
                    {
                        MediaSequence = entry.MediaSequence,
                        PlaylistType = entry.PlayListType,
                        TargetDurationMilliseconds = entry.TargetDuration,
                        Version = entry.Version,
                        UserExcluded = false,

                    };
                    var entityAttributes = new M3UInfoAttributes()
                    {
                        AspectRatio = entry.Attributes.AspectRatio,
                        AudioTrack = entry.Attributes.AudioTrack,
                        Cache = entry.Attributes.Cache,
                        ChannelNumber = entry.Attributes.ChannelNumber,
                        Deinterlace = entry.Attributes.Deinterlace,
                        GroupTitle = entry.Attributes.GroupTitle,
                        M3UAutoLoad = entry.Attributes.M3UAutoLoad,
                        M3UInfo = entity,
                        Refresh = entry.Attributes.Refresh,
                        TvgId = entry.Attributes.TvgId,
                        TvgLogo = entry.Attributes.TvgLogo,
                        TvgName = entry.Attributes.TvgName, 
                        TvgShift = entry.Attributes.TvgShift,
                        UrlTvg = entry.Attributes.UrlTvg    
                    };
                    var entityWarnings = entry.Warnings.Select(warning => new M3UInfoWarning()
                    {
                        M3UInfo = entity,
                        Warning = warning                        
                    });
                    var entityMedia = entry.Medias.Select(media => new M3UMedia()
                    {
                        DurationMilliseconds = decimal.ToInt32(media.Duration),
                        InnerTitle = media.Title.InnerTitle,
                        MediaFile = media.MediaFile,
                        RawTitle = media.Title.RawTitle,
                        UserExcluded = false
                    });

                    context.M3UMedias.AddRange(entityMedia);
                    context.M3UInfoWarnings.AddRange(entityWarnings);
                    context.M3UInfoAttributes.Add(entityAttributes);
                    context.M3UInfos.Add(entity);

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error in IModelController (AddLibraryEntry):  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        //public MusicBrainzRecordViewModel LoadFromMusicBrainz(string musicBrainzId)
        //{
        //    throw new NotImplementedException();
        //}

        public File LoadTag(string fileName)
        {
            throw new NotImplementedException();
        }

        private AudioStationDbContext CreateContext()
        {
            var configuration = _configurationManager.GetConfiguration();

            var connectionString = "Host={0};Database={1};Username={2};Password={3}";

            return new AudioStationDbContext(string.Format(connectionString,
                                                            configuration.DatabaseHost,
                                                            configuration.DatabaseName,
                                                            configuration.DatabaseUser,
                                                            configuration.DatabasePassword), _outputController);
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
