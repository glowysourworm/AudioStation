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

        // Tables
        public DbSet<MusicBrainzArtistEntity> MusicBrainzArtists { get; set; }
        public DbSet<MusicBrainzEntityType> MusicBrainzEntityTypes { get; set; }
        public DbSet<MusicBrainzGenreEntity> MusicBrainzGenres { get; set; }
        public DbSet<MusicBrainzLabelEntity> MusicBrainzLabels { get; set; }
        public DbSet<MusicBrainzMediumEntity> MusicBrainzMedia { get; set; }
        public DbSet<MusicBrainzRecordingEntity> MusicBrainzRecordings { get; set; }
        public DbSet<MusicBrainzReleaseEntity> MusicBrainzReleases { get; set; }
        public DbSet<MusicBrainzTagEntity> MusicBrainzTags { get; set; }
        public DbSet<MusicBrainzTrackEntity> MusicBrainzTracks { get; set; }
        public DbSet<MusicBrainzUrlEntity> MusicBrainzUrls { get; set; }

        // Maps (N-M Tables)
        public DbSet<MusicBrainzArtistRecordingMap> MusicBrainzArtistRecordMaps { get; set; }
        public DbSet<MusicBrainzArtistReleaseMap> MusicBrainzArtistReleaseMaps { get; set; }
        public DbSet<MusicBrainzGenreEntityMap> MusicBrainzGenreEntityMaps { get; set; }
        public DbSet<MusicBrainzLabelReleaseMap> MusicBrainzLabelReleaseMaps { get; set; }
        public DbSet<MusicBrainzTagEntityMap> MusicBrainzTagEntityMaps { get; set; }
        public DbSet<MusicBrainzUrlEntityMap> MusicBrainzUrlEntityMaps { get; set; }

        // Views
        public DbSet<MusicBrainzCombinedRecordView> MusicBrainzCombinedRecordViews { get; set; }

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

            modelBuilder.Entity<MusicBrainzArtistRecordingMap>().Navigation(x => x.MusicBrainzArtist).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzArtistRecordingMap>().Navigation(x => x.MusicBrainzRecording).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzArtistReleaseMap>().Navigation(x => x.MusicBrainzArtist).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzArtistReleaseMap>().Navigation(x => x.MusicBrainzRelease).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzGenreEntityMap>().Navigation(x => x.MusicBrainzGenre).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzGenreEntityMap>().Navigation(x => x.MusicBrainzEntityType).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzLabelReleaseMap>().Navigation(x => x.MusicBrainzLabel).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzLabelReleaseMap>().Navigation(x => x.MusicBrainzRelease).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzMediumEntity>().Navigation(x => x.MusicBrainzRelease).AutoInclude(true);

            // Not sure when / how this loads. This would follow the normal EF pattern.
            modelBuilder.Entity<MusicBrainzReleaseEntity>()
                        .HasMany(x => x.Media)
                        .WithOne(x => x.MusicBrainzRelease)
                        .HasForeignKey(x => x.MusicBrainzReleaseId);

            modelBuilder.Entity<MusicBrainzTagEntityMap>().Navigation(x => x.MusicBrainzEntityType).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzTagEntityMap>().Navigation(x => x.MusicBrainzTag).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzTrackEntity>().Navigation(x => x.MusicBrainzMedium).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzTrackEntity>().Navigation(x => x.MusicBrainzRecording).AutoInclude(true);

            modelBuilder.Entity<MusicBrainzUrlEntityMap>().Navigation(x => x.MusicBrainzEntityType).AutoInclude(true);
            modelBuilder.Entity<MusicBrainzUrlEntityMap>().Navigation(x => x.MusicBrainzUrl).AutoInclude(true);

            // Views
            modelBuilder.Entity<MusicBrainzCombinedRecordView>().ToView("MusicBrainzCombinedRecordView", "public");
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

            var message = _logVerbose ? eventData.ToString() : string.Format("Npgsql Event (Music Brainz Local): Level={0} Id={1} Code={2} Name={3}",
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
