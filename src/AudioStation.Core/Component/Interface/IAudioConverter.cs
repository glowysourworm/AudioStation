using System.IO;

using AudioStation.Core.Model;

using CSCore;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Component for converting audio, and reporting to the application what codecs are supported for reading and
    /// writing.
    /// </summary>
    public interface IAudioConverter
    {
        /// <summary>
        /// (UI TOOL!) This will be set directly into the OpenFileDialog (for filtering supported files)
        /// </summary>
        string GetSupportedFileDialogFilter();

        /// <summary>
        /// Returns entire file search pattern for supported files
        /// </summary>
        string GetSupportedFileSearchPattern();

        /// <summary>
        /// (UI UTIL!) This will return all supported file extensions
        /// </summary>
        IEnumerable<string> GetSupportedFileExtensions();

        /// <summary>
        /// Given standard system types, show what output encodings are supported
        /// </summary>
        IEnumerable<AudioEncoderInfo> GetSupportedFormats();

        /// <summary>
        /// Returns a unique list of extensions for all supported formats
        /// </summary>
        IEnumerable<string> GetSupportedFormatExtensions();

        /// <summary>
        /// Returns default format for audio file extension
        /// </summary>
        AudioEncoderInfo GetDefaultFormat(string formatExtension);

        /// <summary>
        /// Returns the audio format for a particular audio file decoded by the Media Foundation MSFT library
        /// </summary>
        AudioEncoding GetAudioEncoding(string filePath);

        /// <summary>
        /// Returns the stream's audio length - interpreting it using the Media Foundation decoder 
        /// library from CSCore.
        /// </summary>
        int GetDurationMilliseconds(Stream sourceStream);

        /// <summary>
        /// Converts file to requested format
        /// </summary>
        /// <param name="fileIn">Full file path to the input file</param>
        /// <param name="fileOut">Full file path to the output file (THIS MUST MATCH THE CODEC)</param>
        /// <param name="encoderInfo">Supported encodere information</param>
        void ConvertTo(string fileIn, string fileOut, AudioEncoderInfo encoderInfo);

        /// <summary>
        /// (async) Converts file to requested format
        /// </summary>
        /// <param name="fileIn">Full file path to the input file</param>
        /// <param name="fileOut">Full file path to the output file (THIS MUST MATCH THE CODEC)</param>
        /// <param name="encoderInfo">Supported encodere information</param>
        Task ConvertToAsync(string fileNameIn, string fileNameOut, AudioEncoderInfo encoderInfo);
    }
}
