using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzDbContext : DbContext
    {
        private readonly Configuration _configuration;
        private readonly bool _logVerbose;

        // Entity types
        public DbSet<MusicBrainzArtistEntity> MusicBrainzArtists { get; set; }
        public DbSet<MusicBrainzDiscEntity> MusicBrainzDiscs { get; set; }
        public DbSet<MusicBrainzEntityType> MusicBrainzEntityTypes { get; set; }
        public DbSet<MusicBrainzGenreEntity> MusicBrainzGenres { get; set; }
        public DbSet<MusicBrainzLabelEntity> MusicBrainzLabels { get; set; }
        public DbSet<MusicBrainzMediumEntity> MusicBrainzMedia { get; set; }
        public DbSet<MusicBrainzRecordingEntity> MusicBrainzRecordings { get; set; }
        public DbSet<MusicBrainzReleaseEntity> MusicBrainzReleases { get; set; }
        public DbSet<MusicBrainzTagEntity> MusicBrainzTags { get; set; }
        public DbSet<MusicBrainzTrackEntity> MusicBrainzTracks { get; set; }
        public DbSet<MusicBrainzUrlEntity> MusicBrainzUrls { get; set; }

        // Maps
        public DbSet<MusicBrainzArtistRecordingMap> MusicBrainzArtistRecordMaps { get; set; }
        public DbSet<MusicBrainzArtistReleaseMap> MusicBrainzArtistReleaseMaps { get; set; }
        public DbSet<MusicBrainzArtistTrackMap> MusicBrainzArtistTrackMaps { get; set; }
        public DbSet<MusicBrainzGenreEntityMap> MusicBrainzGenreEntityMaps { get; set; }
        public DbSet<MusicBrainzLabelReleaseMap> MusicBrainzLabelReleaseMaps { get; set; }
        public DbSet<MusicBrainzReleaseLabelMap> MusicBrainzReleaseLabelMaps { get; set; }
        public DbSet<MusicBrainzTagEntityMap> MusicBrainzTagEntityMaps { get; set; }
        public DbSet<MusicBrainzUrlEntityMap> MusicBrainzUrlEntityMaps { get; set; }

        public MusicBrainzDbContext(Configuration configuration,
                                    bool logVerbose)

        {
            _configuration = configuration;
            _logVerbose = logVerbose;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<M3UStream>().HasIndex("Name");

            //modelBuilder.Entity<Mp3FileReference>().Navigation(x => x.PrimaryArtist).AutoInclude(true);
            //modelBuilder.Entity<Mp3FileReference>().Navigation(x => x.Album).AutoInclude(true);
            //modelBuilder.Entity<Mp3FileReference>().Navigation(x => x.PrimaryGenre).AutoInclude(true);

            //modelBuilder.Entity<Mp3FileReferenceArtistMap>().Navigation(x => x.Mp3FileReferenceArtist).AutoInclude(true);
            //modelBuilder.Entity<Mp3FileReferenceArtistMap>().Navigation(x => x.Mp3FileReference).AutoInclude(true);

            //modelBuilder.Entity<Mp3FileReferenceGenreMap>().Navigation(x => x.Mp3FileReferenceGenre).AutoInclude(true);
            //modelBuilder.Entity<Mp3FileReferenceGenreMap>().Navigation(x => x.Mp3FileReference).AutoInclude(true);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = GetConnectionString(_configuration, _logVerbose);

            optionsBuilder.UseNpgsql(connectionString, builder =>
            {

            });
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.LogTo(FilterLogging, Log);
            optionsBuilder.EnableThreadSafetyChecks(true);

            base.OnConfiguring(optionsBuilder);
        }

        public static string GetConnectionString(Configuration configuration, bool logVerbose)
        {
            var connectionStringFormat = "Host={0};Database={1};Username={2};Password={3};";

            var connectionString = string.Format(connectionStringFormat,
                                                 configuration.MusicBrainzDatabaseHost,
                                                 configuration.MusicBrainzDatabaseName,
                                                 configuration.MusicBrainzDatabaseUser,
                                                 configuration.MusicBrainzDatabasePassword);

            // Must apply ADO.NET Connection String rules
            var builder = new NpgsqlConnectionStringBuilder(/*connectionString*/);

            // Connection / User Credentials
            builder.Host = configuration.MusicBrainzDatabaseHost;
            builder.Database = configuration.MusicBrainzDatabaseName;
            builder.Username = configuration.MusicBrainzDatabaseUser;
            builder.Password = configuration.MusicBrainzDatabasePassword;

            // Transactions (don't assume ambient transaction scope) (we're pooling; but not using transactions)
            builder.Enlist = false;

            // Logging
            builder.IncludeErrorDetail = logVerbose;

            // Prepared Statements:  https://www.roji.org/prepared-statements-in-npgsql-3-2
            //                       https://www.npgsql.org/doc/performance.html
            //                       https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/retrieving-binary-data?redirectedfrom=MSDN
            //
            builder.MaxAutoPrepare = 0;
            builder.AutoPrepareMinUsages = 1;
            builder.NoResetOnClose = true;
            builder.Pooling = true;

            // Read Buffering (row-level internal buffer)
            builder.WriteBufferSize = 18000;
            builder.ReadBufferSize = 18000;                 // Suggested 18K buffer (per table row, essentially)
            builder.SocketReceiveBufferSize = 18000;        // Not sure about this one... (assuming it's sending it; and there's 
            builder.SocketSendBufferSize = 18000;           //                             performance lag receiving it..?)
            builder.WriteCoalescingBufferThresholdBytes = 18000;

            return builder.ToString();
        }

        /// <summary>
        /// Filters Postgres / EF logging using their callback
        /// </summary>
        private bool FilterLogging(EventId eventId, LogLevel level)
        {
            //if (level >= LogLevel.Information)
            //    return true;

            // Information level includes ALL select, and other DB statements (apparently), so we're going to allow
            // them all through; and do some selective output in the "Log" function
            return true;
        }

        /// <summary>
        /// Log from Postgres / EF
        /// </summary>
        private void Log(EventData eventData)
        {
            // TODO:  We could add configuration options for logging to remove / add developer information (select statements).
            //        For now, lets just include the event codes and say they're part of the 

            var message = _logVerbose ? eventData.ToString() : string.Format("Npgsql Event: Level={0} Id={1} Code={2} Name={3}",
                                                                             Enum.GetName(eventData.LogLevel),
                                                                             eventData.EventId.Id,
                                                                             eventData.EventIdCode,
                                                                             eventData.EventId.Name);

            ApplicationHelpers.Log(message, LogMessageType.Database, eventData.LogLevel);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
