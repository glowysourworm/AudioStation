using System.Buffers;
using System.IO;

using AcoustID.Audio;

using CSCore.MediaFoundation;

namespace AudioStation.Core.Service.Vendor.AcoustIDComponent
{
    /// <summary>
    /// Decode using the NAudio library.
    /// </summary>
    public class NAudioDecoder : AudioDecoder
    {
        string _file;

        public NAudioDecoder(string file)
        {
            _file = file;

            Initialize();
        }

        public override bool Decode(IAudioConsumer consumer, int maxLength)
        {
            using (var stream = File.OpenRead(_file))
            {
                using (var reader = new MediaFoundationDecoder(stream))
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
        }

        private bool Initialize()
        {
            using (var fileStream = File.OpenRead(_file))
            {
                using (var reader = new MediaFoundationDecoder(fileStream))
                {
                    var format = reader.WaveFormat;

                    this.sampleRate = format.SampleRate;
                    this.channels = format.Channels;
                    this.bitsPerSample = format.BitsPerSample;
                    this.totalSeconds = reader.WaveFormat.BytesToMilliseconds(reader.Length) / 1000.0D;

                    return format.BitsPerSample != 16;
                }
            }
        }
    }
}
