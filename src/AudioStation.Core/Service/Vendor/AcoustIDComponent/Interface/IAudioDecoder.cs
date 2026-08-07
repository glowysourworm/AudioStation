using AcoustID.Audio;

namespace AudioStation.Core.Service.Vendor.AcoustIDComponent.Interface
{
    /// <summary>
    /// Interface for audio decoders.
    /// </summary>
    public interface IAudioDecoder : IDecoder, IDisposable
    {
        int BitsPerSample { get; }
        double TotalSeconds { get; }
    }
}
