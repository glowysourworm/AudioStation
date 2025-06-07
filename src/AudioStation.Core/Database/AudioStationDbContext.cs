
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Database
{
    public class AudioStationDbContext : DbContext
    {
        private readonly IOutputController _outputController;
        private readonly string _connectionString;

        public DbSet<M3UInfo> M3UInfos { get; set; }
        public DbSet<M3UInfoAttributes> M3UInfoAttributes { get; set; }
        public DbSet<M3UInfoWarning> M3UInfoWarnings { get; set; }
        public DbSet<M3UMedia> M3UMedias { get; set; }
        public DbSet<Mp3FileReference> Mp3FileReferences { get; set; }
        public DbSet<Mp3FileReferenceAlbum> Mp3FileReferenceAlbums { get; set; }
        public DbSet<Mp3FileReferenceArtist> Mp3FileReferenceArtists { get; set; }
        public DbSet<Mp3FileReferenceGenre> Mp3FileReferenceGenres { get; set; }

        public AudioStationDbContext(string connectionString, IOutputController outputController)

        {
            _connectionString = connectionString;
            _outputController = outputController;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString, builder =>
            {
            });
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.LogTo(FilterLogging, Log);

            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Filters Postgres / EF logging using their callback
        /// </summary>
        private bool FilterLogging(EventId eventId, LogLevel level)
        {
            return true;
        }

        /// <summary>
        /// Log from Postgres / EF
        /// </summary>
        private void Log(EventData eventData)
        {
            _outputController.AddLog(new LogMessage()
            {
                 Message = eventData.ToString(),
                 Severity = eventData.LogLevel == LogLevel.Error ? LogMessageSeverity.Error : LogMessageSeverity.Info,
                 Type = LogMessageType.Database
            });
        }
    }
}
