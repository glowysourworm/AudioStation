using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// This loader object is meant to contain the load and provide type checks
    /// </summary>
    public class LibraryLoaderLoad
    {
        object _load;
        LibraryLoadType _loadType;
        Type _actualType;

        /// <summary>
        /// Loads the loader with the load specification.
        /// </summary>
        public LibraryLoaderLoad(LibraryLoadType loadType, object load)
        {
            Initialize(loadType, load);
        }

        private void Initialize(LibraryLoadType loadType, object load)
        {
            if (load == null)
                throw new NullReferenceException("Library loader load not set");

            switch (loadType)
            {
                case LibraryLoadType.ImportRadio:
                    if (load is not LibraryLoaderFileLoad)
                        throw new ArgumentException("Improper library load type:  ImportRadio expects LibraryLoaderFileLoad");

                    _actualType = typeof(LibraryLoaderFileLoad);
                    break;
                case LibraryLoadType.AcoustID:
                    if (load is not LibraryLoaderFileLoad)
                        throw new ArgumentException("Improper library load type:  AcoustID expects LibraryLoaderFileLoad");

                    _actualType = typeof(LibraryLoaderFileLoad);
                    break;
                case LibraryLoadType.FileChecker:
                    if (load is not LibraryLoaderEntityLoad<FileReference>)
                        throw new ArgumentException("Improper library load type:  FileChecker expects LibraryLoaderEntityLoad<FileReference>");

                    _actualType = typeof(LibraryLoaderEntityLoad<FileReference>);
                    break;
                case LibraryLoadType.MusicBrainzBasic:
                    if (load is not LibraryLoaderEntitySetLoad<AcoustIDLookupResult>)
                        throw new ArgumentException("Improper library load type:  MusicBrainzBasic expects LibraryLoaderEntitySetLoad<AcoustIDLookupResult>");

                    _actualType = typeof(LibraryLoaderEntitySetLoad<AcoustIDLookupResult>);
                    break;
                case LibraryLoadType.MusicBrainzAlbumArt:
                    if (load is not LibraryLoaderEntityLoad<TagSmallVendorMap>)
                        throw new ArgumentException("Improper library load type:  MusicBrainzAlbumArt expects LibraryLoaderEntityLoad<TagSmallVendorMap>");

                    _actualType = typeof(LibraryLoaderEntityLoad<TagSmallVendorMap>);
                    break;
                case LibraryLoadType.Import:
                    if (load is not LibraryLoaderFileLoad)
                        throw new ArgumentException("Improper library load type:  Import expects LibraryLoaderImportLoad");

                    _actualType = typeof(LibraryLoaderImportLoad);
                    break;
                default:
                    throw new Exception("Unhandled library load type");
            }

            _load = load;
            _loadType = loadType;
        }

        /// <summary>
        /// Gets load, casted as the appropriate object
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) != _actualType)
                throw new ArgumentException("Load type is not not correct, expecting:  " + _actualType);

            return (T)_load;
        }
    }
}
