using AudioStation.Core.Model.Interface;

using CSCore;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Encoding info for converting audio files
    /// </summary>
    public class AudioEncoderInfo : IAudioEncoderInfo
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Filter { get; set; }
        public AudioEncoding Encoding { get; set; }

        public AudioEncoderInfo()
        {
            this.Name = string.Empty;
            this.Extension = string.Empty;
            this.Filter = string.Empty;
            this.Encoding = AudioEncoding.Unknown;
        }
    }
}
