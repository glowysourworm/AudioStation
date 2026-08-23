using AcoustID.Web;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.AcoustIDComponent;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IAcoustIDClient))]
    public class AcoustIDClient : IAcoustIDClient, IAudioStationService
    {
        private readonly IConfigurationManager _configurationManager;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

        [IocImportingConstructor]
        public AcoustIDClient(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _status = IAudioStationService.Status.Disabled;
        }

        /// <summary>
        /// Calculates library entry by audio fingerprint using an online api.
        /// </summary>
        public Task<IEnumerable<AcoustIDLookupResult>> IdentifyFingerprint(string fileName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // -> Working
                    OnStatusChanged(IAudioStationService.Status.Working);

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

                    var response = await service.GetAsync(fingerPrint, length, availableMeta);

                    // -> Idle
                    OnStatusChanged(IAudioStationService.Status.Idle);

                    return response.Results
                                   .Where(x => x.Score >= (minScore / 100.0D))
                                   .Where(x => x.Recordings != null && x.Recordings.Any())
                                   .OrderByDescending(x => x.Score)
                                   .SelectMany(x =>
                                   {
                                       var results = new List<AcoustIDLookupResult>();

                                       foreach (var recording in x.Recordings)
                                       {
                                           results.Add(new AcoustIDLookupResult()
                                           {
                                               AcoustIDChromaPrint = new AcoustIDChromaPrint()
                                               {
                                                   Fingerprint = fingerPrint
                                               },
                                               Fingerprint = fingerPrint,
                                               LookupId = new Guid(x.Id),
                                               MusicBrainzRecordingId = new Guid(recording.Id),
                                               Score = x.Score
                                           });
                                       }

                                       return results;
                                   })
                                   .ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error using AcoustID service:  {0}", LogMessageServiceType.AcoustID, LogLevel.Error, ex, ex.Message);

                    return Enumerable.Empty<AcoustIDLookupResult>();
                }
            });
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Acoust ID Client";
        }
        public string GetDisplayName()
        {
            return "Acoust ID Client";
        }
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public async Task<IAudioStationService.Status> Initialize()
        {
            if (string.IsNullOrWhiteSpace(_configurationManager.GetConfiguration().AcoustIDAPIKey))
                return _status;

            // Setup Static Configuration
            AcoustID.Configuration.ClientKey = _configurationManager.GetConfiguration().AcoustIDAPIKey;

            // -> Idle
            OnStatusChanged(IAudioStationService.Status.Idle);

            return _status;
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}
