using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.MediaFoundation;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IAudioConverter))]
    public class AudioConverter : IAudioConverter
    {
        public AudioConverter()
        {

        }

        public void ConvertTo(string filePathIn, string filePathOut, AudioEncoderInfo outputEncoding)
        {
            if (string.IsNullOrWhiteSpace(filePathIn))
                throw new ArgumentException("Invalid input file");

            if (string.IsNullOrWhiteSpace(filePathOut))
                throw new ArgumentException("Invalid input file");

            if (Path.GetExtension(filePathOut) != outputEncoding.Extension)
                throw new ArgumentException("Invalid output file: extension does not match valid OS audio file extension");

            try
            {
                using (var fileStream = File.OpenRead(filePathIn))
                {
                    using (var source = new MediaFoundationDecoder(fileStream))
                    {
                        // -> Output Format
                        var format = new WaveFormat(source.WaveFormat.SampleRate,
                                                    source.WaveFormat.BitsPerSample,
                                                    source.WaveFormat.Channels,
                                                    outputEncoding.Encoding);

                        using (var encoder = new WaveWriter(filePathOut, format))
                        {
                            // -> One Second Buffer
                            byte[] buffer = new byte[source.WaveFormat.BytesPerSecond];
                            int read;

                            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                encoder.Write(buffer, 0, read);

                                // TODO: Progress Callback
                                //Console.CursorLeft = 0;
                                //Console.Write("{0:P}/{1:P}", (double)source.Position / source.Length, 1);
                            }
                        }
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

        public AudioEncoding GetAudioEncoding(string filePath)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    using (var source = new MediaFoundationDecoder(fileStream))
                    {
                        return source.WaveFormat.WaveFormatTag;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error converting audio file:  " + ex.Message, LogLevel.Error, ex, null);
                throw ex;
            }
        }

        public string GetSupportedFileDialogFilter()
        {
            return CodecFactory.SupportedFilesFilterEn;
        }

        public IEnumerable<string> GetSupportedFileExtensions()
        {
            return CodecFactory.Instance.GetSupportedFileExtensions();
        }

        public string GetSupportedFileSearchPattern()
        {
            return CodecFactory.Instance.GetSupportedFileExtensions().Select(x => "*." + x).Join("|", x => x);
        }

        public IEnumerable<AudioEncoderInfo> GetSupportedFormats()
        {
            return new List<AudioEncoderInfo>
            {
                // MP3
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.MpegLayer3,
                    Extension = ".mp3",
                    Filter = "*.mp3",
                    Name = "Mpeg Layer 3"
                },

                // WMA
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.WindowsMediaAudio,
                    Extension = ".wma",
                    Filter = "*.wma",
                    Name = "Windows Media Audio"
                },
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.WindowsMediaAudioLosseless,
                    Extension = ".wma",
                    Filter = "*.wma",
                    Name = "Windows Media Audio (Lossless)"
                },
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.WindowsMediaAudioProfessional,
                    Extension = ".wma",
                    Filter = "*.wma",
                    Name = "Windows Media Audio (Professional)"
                },
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.WindowsMediaAudioSpdif,
                    Extension = ".wma",
                    Filter = "*.wma",
                    Name = "Windows Media Audio (Spdif)"
                },

                //WAV
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.Pcm,
                    Extension = ".wav",
                    Filter = "*.wav",
                    Name = "Wave File"
                },

                // FLAC
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.WAVE_FORMAT_FLAC,
                    Extension = ".flac",
                    Filter = "*.flac",
                    Name = "Free Lossless Audio Codec"
                },

                
                // AAC
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.RawAac,
                    Extension = ".aac",
                    Filter = "*.aac",
                    Name = "Advanced Audio Codec (Raw)"
                },
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.MPEG_RAW_AAC,
                    Extension = ".aac",
                    Filter = "*.aac",
                    Name = "Advanced Audio Codec (Mpeg Raw)"
                },

                // RAW
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.Pcm,
                    Extension = ".raw",
                    Filter = "*.raw",
                    Name = "CD-Audio (PCM 16-bit)"
                },

                // OGG
                new AudioEncoderInfo()
                {
                    Encoding = AudioEncoding.Vorbis1,
                    Extension = ".raw",
                    Filter = "*.raw",
                    Name = "Vorbis (version 1)"
                }
            };
        }
    }
}
