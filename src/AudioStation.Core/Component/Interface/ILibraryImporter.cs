using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput.Interface;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Component for processing import work load
    /// </summary>
    public interface ILibraryImporter
    {
        public bool CanImportAcoustID(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        public bool CanImportMusicBrainzBasic(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        public bool CanImportMusicBrainzDetail(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        public bool CanImportEmbedTag(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        public bool CanImportEntity(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        public bool CanImportMigrateFile(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);

        Task<bool> WorkAcoustID(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        Task<bool> WorkMusicBrainzDetail(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        Task<bool> WorkMusicBrainzCompleteRecord(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        bool WorkEmbedTag(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        bool WorkImportEntity(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
        bool WorkMigrateFile(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput);
    }
}
