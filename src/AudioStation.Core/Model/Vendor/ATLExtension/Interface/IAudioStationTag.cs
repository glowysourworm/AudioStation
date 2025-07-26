using ATL.AudioData;

namespace AudioStation.Core.Model.Vendor.ATLExtension.Interface
{
    /// <summary>
    /// Marker interface for our namespace <--> ATL. This represents an ATL.Track entity (closely), or an ATL.IMetaData
    /// instance.
    /// </summary>
    public interface IAudioStationTag : IMetaData
    {
        /// <summary>
        /// Updates calculated files that aren't set by the edit process
        /// </summary>
        void UpdateAfterEdit();

        /// <summary>
        /// Updates fields that aren't going to be set by the edit process
        /// </summary>
        void UpdateBeforeEdit();

        /// <summary>
        /// Collection of album artists (AlbumArtist (singular) will be the first of this collection)
        /// </summary>
        IList<string> AlbumArtists { get; }

        /// <summary>
        /// This is the name of the audio format from the ATL.Track data.
        /// </summary>
        string AudioFormat { get; }

        /// <summary>
        /// Bit depth from the ATL.Track data.
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// Bit rate from the ATL.Track data.
        /// </summary>
        int BitRate { get; }

        /// <summary>
        /// Number of channels from the ATL.Track data.
        /// </summary>
        int Channels { get; }

        /// <summary>
        /// Duration of the audio in milli-second precision (as drawn from
        /// the ATL library).
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Collection of genres (Genre (singular) will be the first of this collection)
        /// </summary>
        IList<string> Genres { get; }

        /// <summary>
        /// Variable bitrate flag from ATL.Track data.
        /// </summary>
        bool IsVariableBitRate { get; }

        /// <summary>
        /// Sample rate from the ATL.Track data.
        /// </summary>
        double SampleRate { get; }

        /// <summary>
        /// Parsed track integer from the ATL.Track field TrackNumber (string)
        /// </summary>
        uint Track { get; }

        /// <summary>
        /// Year from the ATL.Track data. This is probably used by most older Mp3 software - so
        /// we're adding it as such. The Date field is ambiguous. Is it a file date? Is it the
        /// release date? We're going to use the Date field as the file creation date. The year
        /// may be folded into the PublishingDate (at a later time)
        /// </summary>
        int Year { get; }
    }
}
