using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Controller;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Component for processing import work load
    /// </summary>
    public interface ILibraryImporter
    {
        public bool CanImportAcoustID(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        public bool CanImportMusicBrainzBasic(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        public bool CanImportMusicBrainzDetail(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        public bool CanImportEmbedTag(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        public bool CanImportEntity(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        public bool CanImportMigrateFile(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);

        Task<bool> WorkAcoustID(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        Task<bool> WorkMusicBrainzDetail(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        Task<bool> WorkMusicBrainzCompleteRecord(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        bool WorkEmbedTag(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        Task<bool> WorkImportEntity(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
        Task<bool> WorkMigrateFile(LibraryLoaderImportLoad workInput, LibraryLoaderImportOutput workOutput);
    }
}
