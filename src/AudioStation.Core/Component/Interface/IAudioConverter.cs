namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Component for converting audio, and reporting to the application what codecs are supported for reading and
    /// writing.
    /// </summary>
    public interface IAudioConverter
    {
        /// <summary>
        /// Given standard system types, show what output encodings are supported
        /// </summary>
        IEnumerable<AudioEncoderInfo> GetSupportedFormats();

        /// <summary>
        /// Given an audio input type - show what output formats are supported
        /// </summary>
        IEnumerable<AudioEncoderInfo> GetSupportedOutputFormats(string file);

        void ConvertTo(string fileIn, string fileOut, AudioEncoderInfo encoderInfo);

        Task ConvertToAsync(string fileNameIn, string fileNameOut, AudioEncoderInfo encoderInfo);
    }
}
