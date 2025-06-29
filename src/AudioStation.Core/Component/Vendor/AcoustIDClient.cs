using AcoustID.Web;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IAcoustIDClient))]
    public class AcoustIDClient : IAcoustIDClient
    {
        private readonly IConfigurationManager _configurationManager;

        [IocImportingConstructor]
        public AcoustIDClient(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        /// <summary>
        /// Calculates library entry by audio fingerprint using an online api.
        /// </summary>
        public Task<IEnumerable<Recording>> IdentifyFingerprint(string fileName, int minScore)
        {
            // Setup Static Configuration
            SetConfiguration();

            return Task<IEnumerable<Recording>>.Run(async () =>
            {
                try
                {
                    var context = new AcoustID.ChromaContext();
                    var buffer = new short[1000000];
                    var samplingRate = 0;
                    var channels = 0;
                    using (var reader = new Mp3FileReader(fileName))
                    {
                        while (reader.CanRead)
                        {
                            var frame = reader.ReadNextFrame();

                            // Copy from short[] to byte[]
                            Array.Copy(frame.RawData, buffer, frame.FrameLength);

                            // Feed to the chroma context
                            context.Feed(buffer, buffer.Length);

                            samplingRate = frame.SampleRate;
                            channels = (frame.ChannelMode == ChannelMode.Stereo ||
                                        frame.ChannelMode == ChannelMode.DualChannel ||
                                        frame.ChannelMode == ChannelMode.Stereo) ? 2 : 1;
                        }
                    }

                    // Start context
                    context.Start(samplingRate, channels);
                    context.Finish();
                    var fingerPrint = context.GetFingerprint();

                    var service = new LookupService();

                    var response = await service.GetAsync(fingerPrint, 30);

                    return response.Results
                                   .Where(x => x.Score >= minScore)
                                   .OrderByDescending(x => x.Score)
                                   .SelectMany(x => x.Recordings)
                                   .ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error using AcoustID service:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);

                    return Enumerable.Empty<Recording>();
                }
            });
        }

        private void SetConfiguration()
        {
            AcoustID.Configuration.ClientKey = _configurationManager.GetConfiguration().AcoustIDAPIKey;
        }
    }
}
