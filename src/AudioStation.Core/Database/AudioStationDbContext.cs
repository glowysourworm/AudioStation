
using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database
{
    public class AudioStationDbContext : DbContext
    {
        private readonly string _connectionString;

        public DbSet<M3UInfo> M3UInfos { get; set; }
        public DbSet<M3UInfoAttributes> M3UInfoAttributes { get; set; }
        public DbSet<M3UInfoWarning> M3UInfoWarnings { get; set; }
        public DbSet<M3UMedia> M3UMedias { get; set; }
        public DbSet<Mp3FileReference> Mp3FileReferences { get; set; }
        public DbSet<Mp3FileReferenceAlbum> Mp3FileReferenceAlbums { get; set; }
        public DbSet<Mp3FileReferenceArtist> Mp3FileReferenceArtists { get; set; }
        public DbSet<Mp3FileReferenceGenre> Mp3FileReferenceGenres { get; set; }

        public AudioStationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
