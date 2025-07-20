using System;
using System.Buffers;
using System.IO;
using System.Text;

using AcoustID.Audio;
using AcoustID.Web;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    /// <summary>
    /// Interface for audio decoders.
    /// </summary>
    public interface IAudioDecoder : IDecoder, IDisposable
    {
        int BitsPerSample { get; }
        double TotalSeconds { get; }
    }

    /// <summary>
    /// Abstract base class for audio decoders
    /// </summary>
    public abstract class AudioDecoder : IAudioDecoder
    {
        protected static readonly int BUFFER_SIZE = 2 * 192000;

        protected int sampleRate;
        protected int channels;
        protected int bitsPerSample;
        protected double totalSeconds;

        public int SampleRate
        {
            get { return sampleRate; }
        }

        public int Channels
        {
            get { return channels; }
        }

        public int BitsPerSample
        {
            get { return bitsPerSample; }
        }

        public double TotalSeconds
        {
            get { return  totalSeconds; }
        }

        ~AudioDecoder() => Dispose(false);

        public abstract bool Decode(IAudioConsumer consumer, int maxLength);

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    /// <summary>
    /// Decode using the NAudio library.
    /// </summary>
    public class NAudioDecoder : AudioDecoder
    {
        string file;

        public NAudioDecoder(string file)
        {
            this.file = file;

            Initialize();
        }

        public override bool Decode(IAudioConsumer consumer, int maxLength)
        {
            using (var reader = OpenWaveStream(file))
            {
                if (reader.WaveFormat.BitsPerSample != 16)
                {
                    return false;
                }

                int remaining, length, size;

                var buffer = ArrayPool<byte>.Shared.Rent(2 * BUFFER_SIZE);
                var data = ArrayPool<short>.Shared.Rent(BUFFER_SIZE);

                // Samples to read to get maxLength seconds of audio
                remaining = maxLength * this.Channels * this.sampleRate;

                // Bytes to read
                length = 2 * Math.Min(remaining, BUFFER_SIZE);

                while ((size = reader.Read(buffer, 0, length)) > 0)
                {
                    Buffer.BlockCopy(buffer, 0, data, 0, size);

                    consumer.Consume(data, size / 2);

                    remaining -= size / 2;
                    if (remaining <= 0)
                    {
                        break;
                    }

                    length = 2 * Math.Min(remaining, BUFFER_SIZE);
                }

                ArrayPool<byte>.Shared.Return(buffer);
                ArrayPool<short>.Shared.Return(data);

                return true;
            }
        }

        private bool Initialize()
        {
            using (var reader = OpenWaveStream(file))
            {
                var format = reader.WaveFormat;

                this.sampleRate = format.SampleRate;
                this.channels = format.Channels;
                this.bitsPerSample = format.BitsPerSample;
                this.totalSeconds = reader.TotalTime.TotalSeconds;

                return format.BitsPerSample != 16;
            }
        }

        private WaveStream OpenWaveStream(string file)
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();

            if (extension.Equals(".mp3"))
            {
                return new Mp3FileReader(file);
            }

            // Try open as WAV (will throw an exception, if not supported).
            return new WaveFileReader(file);
        }
    }

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
        public Task<IEnumerable<LookupResult>> IdentifyFingerprint(string fileName, int minScore)
        {
            // Setup Static Configuration
            SetConfiguration();

            return Task<IEnumerable<LookupResult>>.Run(async () =>
            {
                try
                {
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

                    return response.Results
                                   .Where(x => x.Score >= (minScore / 100.0D))
                                   .OrderByDescending(x => x.Score)
                                   .ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error using AcoustID service:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);

                    return Enumerable.Empty<LookupResult>();
                }
            });
        }

        private void SetConfiguration()
        {
            AcoustID.Configuration.ClientKey = _configurationManager.GetConfiguration().AcoustIDAPIKey;
        }
    }
}
