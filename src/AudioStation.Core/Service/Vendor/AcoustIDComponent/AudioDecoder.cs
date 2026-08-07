using AcoustID.Audio;

using AudioStation.Core.Service.Vendor.AcoustIDComponent.Interface;

namespace AudioStation.Core.Service.Vendor.AcoustIDComponent
{
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
            get { return totalSeconds; }
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
}
