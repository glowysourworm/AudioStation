using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;

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
            var configuration = _configurationManager.GetConfiguration();

            // Procedure: The Library and Radio components are loaded from
            //            any database data that exists. At runtime, the rest
            //            of the data may be queried from this controller.
            //
            this.Library = new Library();
            this.Radio = new Radio();

            var connectionString = "Host={0};Database={1};Username={2};Password={3}";

            using (var context = new AudioStationDbContext(string.Format(connectionString,
                                                                         configuration.DatabaseHost,
                                                                         configuration.DatabaseName,
                                                                         configuration.DatabaseUser,
                                                                         configuration.DatabasePassword), _outputController))
            {
                // Load data from the database that will be used to group / view entries. The
                // rest of the data will be loaded lazily. The mp3 tag data will be called once
                // the view loads the Album, Artist, or Entry (views). So, don't discard the 
                // data. Flag the Library (local entites) that they have been read / not-read.

                var fileRefs = context.Mp3FileReferences.Select(x => new LibraryEntry()
                {
                    Album = x.Album,
                    FileName = x.FileName,
                    PrimaryArtist = x.PrimaryArtist,
                    Title = x.Title,
                    Track = (uint)x.Track

                }).Distinct().ToList();
                var albums = context.Mp3FileReferenceAlbums.Select(x => new Album()
                {
                    Name = x.Name

                }).Distinct().ToList();
                var artists = context.Mp3FileReferenceArtists.Select(x => new Artist()
                {
                    Name = x.Name,

                }).Distinct().ToList();

                var genres = context.Mp3FileReferenceGenres.Select(x => x.Name).Distinct().ToList();

                // Aggregated data
                foreach (var album in albums)
                {
                    album.Tracks = fileRefs.Where(x => x.Album == album.Name).ToList();
                }
                foreach (var artist in artists)
                {
                    artist.Albums = albums.Where(album => album.Tracks.All(track => track.PrimaryArtist == artist.Name)).ToList();
                }
                foreach (var fileRef in fileRefs)
                {
                    var record = context.Mp3FileReferenceAlbums.First(x => x.Mp3FileReferenceId == fileRef.Id);
                    var recordGenre = context.Mp3FileReferenceGenres.First(x => x.Mp3FileReferenceId == fileRef.Id);

                    fileRef.Disc = (uint)record.DiscNumber;
                    fileRef.PrimaryGenre = recordGenre.Name;
                }

                this.Library.Albums = albums;
                this.Library.Artists = artists;
                this.Library.Genres = genres;
                this.Library.Titles = fileRefs;
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

        public void Dispose()
        {
            // TODO
        }
    }
}
