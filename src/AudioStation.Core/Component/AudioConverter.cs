using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;

using Microsoft.Extensions.Logging;

using NAudio.MediaFoundation;
using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IAudioConverter))]
    public class AudioConverter : IAudioConverter
    {
        public AudioConverter()
        {

        }

        public void ConvertTo(string fileNameIn, string fileNameOut, AudioEncoderInfo encoderInfo)
        {
            try
            {
                // NAudio Media Foundation Reader (Windows)
                using (var reader = new MediaFoundationReader(fileNameIn))
                {
                    using (var writer = new MediaFoundationEncoder(encoderInfo.NAudioType))
                    {
                        writer.Encode(fileNameOut, reader);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error converting audio file:  " + ex.Message, LogLevel.Error, ex, null);
                throw ex;
            }
        }

        public Task ConvertToAsync(string fileNameIn, string fileNameOut, AudioEncoderInfo encoderInfo)
        {
            return Task.Run(() =>
            {
                ConvertTo(fileNameIn, fileNameOut, encoderInfo);
            });
        }

        public IEnumerable<AudioEncoderInfo> GetSupportedFormats()
        {
            // NAudio:  This mess of code seems to have an example putting together the user end
            //          information you'd usually use:  file extension, encoding type, bitrate, ...
            //
            return new List<AudioEncoderInfo>
            {
                new AudioEncoderInfo() { Name = "AAC", SubTypeId = AudioSubtypes.MFAudioFormat_AAC, Extension = ".mp4" }, // Windows 8 can do a .aac extension as well
                new AudioEncoderInfo() { Name = "Apple Lossless (ALAC)", SubTypeId = AudioSubtypes.MFAudioFormat_ALAC, Extension = ".m4a" },
                new AudioEncoderInfo() { Name = "MP3", SubTypeId = AudioSubtypes.MFAudioFormat_MP3, Extension = ".mp3" },
                new AudioEncoderInfo() { Name = "Windows Media Audio", SubTypeId = AudioSubtypes.MFAudioFormat_WMAudioV8, Extension = ".wma" },
                new AudioEncoderInfo() { Name = "Windows Media Audio Professional", SubTypeId = AudioSubtypes.MFAudioFormat_WMAudioV9, Extension = ".wma" },
                new AudioEncoderInfo() { Name = "Windows Media Audio Voice", SubTypeId = AudioSubtypes.MFAudioFormat_MSP1, Extension = ".wma" },
                new AudioEncoderInfo() { Name = "Windows Media Audio Lossless", SubTypeId = AudioSubtypes.MFAudioFormat_WMAudio_Lossless, Extension = ".wma" },
                new AudioEncoderInfo() { Name = "FLAC", SubTypeId = AudioSubtypes.MFAudioFormat_FLAC, Extension = ".flac" }
            };
        }

        public IEnumerable<AudioEncoderInfo> GetSupportedOutputFormats(string file)
        {
            try
            {
                // NAudio Media Foundation Reader (Windows)
                var reader = new MediaFoundationReader(file);

                // Use wave format creation of media type data
                var mediaType = new MediaType(reader.WaveFormat);

                var subTypes = MediaFoundationEncoder.GetOutputMediaTypes(mediaType.SubType);

                return subTypes.Select(x => new AudioEncoderInfo()
                {
                    Extension = "",
                    TypeId = x.MajorType,
                    Name = x.MediaFoundationObject.ToString() ?? "",
                    SubTypeId = x.SubType,
                    NAudioType = x

                }).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error converting audio file:  " + ex.Message, LogLevel.Error, ex, null);
                throw ex;
            }
        }
    }
}
