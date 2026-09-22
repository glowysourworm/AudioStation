using AcoustID.Web;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.AcoustIDComponent;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IAcoustIDClient))]
    public class AcoustIDClient : VendorServiceBase, IAcoustIDClient
    {
        // Configuration Properties
        double _acoustIDMinScore;
        int _acoustIDMaxResults;

        [IocImportingConstructor]
        public AcoustIDClient() : base("Acoust ID Client", "Acoust ID Client")
        {
        }

        /// <summary>
        /// Calculates library entry by audio fingerprint using an online api.
        /// </summary>
        public Task<IEnumerable<AcoustIDLookupResult>> IdentifyFingerprintAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                return IdentifyFingerprint(fileName);
            });
        }

        /// <summary>
        /// Calculates library entry by audio fingerprint using an online api.
        /// </summary>
        public IEnumerable<AcoustIDLookupResult> IdentifyFingerprint(string fileName)
        {
            ServiceWait();

            try
            {
                // -> Working
                OnStatusChanged(IAudioStationDataService.Status.Working);

                var context = new AcoustID.ChromaContext();
                var buffer = new short[1000000];
                var length = 0;

                using (var decoder = new NAudioDecoder(fileName))
                {
                    length = (int)Math.Ceiling(decoder.TotalSeconds);
                    context.Start(decoder.SampleRate, decoder.Channels);
                    decoder.Decode(context, length);
                    context.Finish();
                }

                var fingerPrint = context.GetFingerprint();

                var service = new LookupService();
                var availableMeta = new string[]{ "recordings",
                                                  "recordingids",
                                                  "releases",
                                                  "releaseids",
                                                  "releasegroups",
                                                  "releasegroupids",
                                                  "tracks",
                                                  "compress",
                                                  "usermeta",
                                                  "sources" };

                var response = service.GetAsync(fingerPrint, length, availableMeta).Result;

                // -> Idle
                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return response.Results
                               .Where(x => x.Score >= _acoustIDMinScore)
                               .Where(x => x.Recordings != null && x.Recordings.Any())
                               .OrderByDescending(x => x.Score)
                               .SelectMany(x =>
                               {
                                   var results = new List<AcoustIDLookupResult>();

                                   foreach (var recording in x.Recordings)
                                   {
                                       results.Add(new AcoustIDLookupResult()
                                       {
                                           FileName = fileName,
                                           LookupId = new Guid(x.Id),
                                           MusicBrainzRecordingId = new Guid(recording.Id),
                                           Score = x.Score
                                       });
                                   }

                                   return results;
                               })
                               .Take(_acoustIDMaxResults)
                               .ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error using AcoustID service:  {0}", LogMessageServiceType.AcoustID, LogLevel.Error, ex, ex.Message);

                // Create empty result with the error message
                return Enumerable.Empty<AcoustIDLookupResult>();
            }
        }

        #region (public) IAudioStationComponent Methods
        public override IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.AcoustIDAPIKey))
            {
                return base.Initialize(configuration);
            }

            // Setup Static Configuration
            AcoustID.Configuration.ClientKey = configuration.AcoustIDAPIKey;

            _acoustIDMaxResults = configuration.AcoustIDMaxResults;
            _acoustIDMinScore = configuration.AcoustIDMinScore;

            // Wait period between calls
            SetThrottleLimit((uint)configuration.AcoustIDWaitMilliseconds);

            // -> Idle
            OnStatusChanged(IAudioStationDataService.Status.Idle);

            // -> Return Status
            return base.Initialize(configuration);
        }
        #endregion
    }
}
