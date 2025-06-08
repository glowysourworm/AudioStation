using AudioStation.Core.Model;

using m3uParser.Model;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelController : IDisposable
    {
        /// <summary>
        /// Method to initialize the controller. This must be called prior to usage.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Collection of primary library of mp3 file references. The rest of the mp3 data may be
        /// loaded using the IModelController.
        /// </summary>
        Library Library { get; }

        /// <summary>
        /// Collection of radio stream data. Additional data my be loaded using online data services.
        /// </summary>
        Radio Radio { get; }

        /// <summary>
        /// Adds LibraryEntry to database. Does NOT update any existing, similar, entry. The tag data
        /// is also used to initialize the LibraryEntry, adding supporting data to the database.
        /// </summary>
        void AddUpdateLibraryEntry(string fileName, TagLib.File tagRef);

        /// <summary>
        /// Adds RadioEntry to database. Does NOT update any existing, similar, entry.
        /// </summary>
        void AddRadioEntry(Extm3u entry);

        /// <summary>
        /// Loads tag data from file. This data does NOT get added to the library.
        /// </summary>
        TagLib.File LoadTag(string fileName);

        /// <summary>
        /// Loads music brainz record. The ID for this sometimes is stored inside the Mp3 tag data. If
        /// it is not there, you can load it using their web-service, or other tools. Modify your tag
        /// data to include the music brainz ID - then you may load the rest of their data at runtime
        /// using this method.
        /// </summary>
        //MusicBrainzRecordViewModel LoadFromMusicBrainz(string musicBrainzId);
    }
}
