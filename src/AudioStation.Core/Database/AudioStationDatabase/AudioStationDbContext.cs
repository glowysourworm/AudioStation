using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    public class AudioStationDbContext : DbContext, IDisposable
    {
        private readonly Configuration _configuration;
        private readonly LogLevel _currentLogLevel;
        private readonly bool _logVerbose;

        public DbSet<AcoustIDLookupResult> AcoustIDLookupResults { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<FileReference> FileReferences { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<M3UStream> M3UStreams { get; set; }
        public DbSet<RadioBrowserStation> RadioBrowserStations { get; set; }
        public DbSet<TagSmall> TagSmalls { get; set; }
        public DbSet<TagSmallVendorMap> TagSmallVendorMaps { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<TrackArtistMap> TrackArtistMaps { get; set; }
        public DbSet<TrackGenreMap> TrackGenreMaps { get; set; }
        public DbSet<Vendor> Vendors { get; set; }


        public AudioStationDbContext(Configuration configuration,
                                     LogLevel currentLogLevel,
                                     bool logVerbose)

        {
            _configuration = configuration;
            _currentLogLevel = currentLogLevel;
            _logVerbose = logVerbose;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add these to use the DbSet methods with templates (!) Very important; and useful!
            //
            modelBuilder.Entity<M3UStream>().HasIndex("Name");

            modelBuilder.Entity<Track>().Navigation(x => x.PrimaryArtist).AutoInclude(true);
            modelBuilder.Entity<Track>().Navigation(x => x.Album).AutoInclude(true);
            modelBuilder.Entity<Track>().Navigation(x => x.PrimaryGenre).AutoInclude(true);

            modelBuilder.Entity<Album>();
            modelBuilder.Entity<Artist>();
            modelBuilder.Entity<Genre>();
            modelBuilder.Entity<RadioBrowserStation>();

            modelBuilder.Entity<TrackArtistMap>().Navigation(x => x.Artist).AutoInclude(true);
            modelBuilder.Entity<TrackArtistMap>().Navigation(x => x.Track).AutoInclude(true);

            modelBuilder.Entity<TrackGenreMap>().Navigation(x => x.Genre).AutoInclude(true);
            modelBuilder.Entity<TrackGenreMap>().Navigation(x => x.Track).AutoInclude(true);

            modelBuilder.Entity<TagSmallVendorMap>().Navigation(x => x.Vendor).AutoInclude(true);
            modelBuilder.Entity<TagSmallVendorMap>().Navigation(x => x.TagSmall).AutoInclude(true);
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
                                                 configuration.DatabaseHost,
                                                 configuration.DatabaseName,
                                                 configuration.DatabaseUser,
                                                 configuration.DatabasePassword);

            // Must apply ADO.NET Connection String rules
            var builder = new NpgsqlConnectionStringBuilder(/*connectionString*/);

            // Connection / User Credentials
            builder.Host = configuration.DatabaseHost;
            builder.Database = configuration.DatabaseName;
            builder.Username = configuration.DatabaseUser;
            builder.Password = configuration.DatabasePassword;

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
            return level >= LogLevel.Information;

            // Information level includes ALL select, and other DB statements (apparently), so we're going to allow
            // them all through; and do some selective output in the "Log" function
            //return true;
        }

        /// <summary>
        /// Log from Postgres / EF
        /// </summary>
        private void Log(EventData eventData)
        {
            // TODO:  We could add configuration options for logging to remove / add developer information (select statements).
            //        For now, lets just include the event codes and say they're part of the 

            // https://github.com/dotnet/efcore/issues/36313
            // https://github.com/npgsql/efcore.pg/issues/3559#issuecomment-3015004260
            var message = /*_logVerbose ? eventData.ToString() : */string.Format("Npgsql Event (Audio Station Db): Level={0} Id={1} Code={2} Name={3}",
                                                                             Enum.GetName(eventData.LogLevel),
                                                                             eventData.EventId.Id,
                                                                             eventData.EventIdCode,
                                                                             eventData.EventId.Name);


            ApplicationHelpers.Log(message, LogMessageDbType.AudioStation, eventData.LogLevel, null);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
