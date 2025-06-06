using AudioStation.Core.Model;
using AudioStation.ViewModel;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    public interface IModelController : IDisposable
    {
        public event SimpleEventHandler<string, LogMessageType, LogMessageSeverity> LogEvent;

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
        /// Loads tag data from file. This data does NOT get added to the library.
        /// </summary>
        TagLib.File LoadTag(string fileName);

        /// <summary>
        /// Loads music brainz record. The ID for this sometimes is stored inside the Mp3 tag data. If
        /// it is not there, you can load it using their web-service, or other tools. Modify your tag
        /// data to include the music brainz ID - then you may load the rest of their data at runtime
        /// using this method.
        /// </summary>
        MusicBrainzRecordViewModel LoadFromMusicBrainz(string musicBrainzId);
    }
}
