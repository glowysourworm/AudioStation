using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryLoadType
    {
        /// <summary>
        /// Opens mp3 file, and adds it to the Mp3FileReference collections in the database. This 
        /// should be run on startup to update any file related information. 
        /// </summary>
        Mp3FileAddUpdate,

        /// <summary>
        /// Opens m3u file, and adds it to the M3UStream table int the database.
        /// </summary>
        M3UFileAddUpdate
    }
    public enum LibraryWorkItemState
    {
        Pending = 0,
        Processing = 1,
        CompleteSuccessful = 2,
        CompleteError = 3
    }
    public class LibraryLoaderWorkItem
    {
        public int Id { get; private set; }
        public string FileName { get; private set; }
        public string ErrorMessage { get; private set; }
        public LibraryLoadType LoadType { get; private set; }
        public LibraryWorkItemState LoadState { get; private set; }

        // Default constructor used for .Equals comparison / FirstOrDefault / etc...
        public LibraryLoaderWorkItem()
        {
            this.Id = -1;
            this.FileName = string.Empty;
            this.LoadType = LibraryLoadType.Mp3FileAddUpdate;
            this.LoadState = LibraryWorkItemState.Pending;
            this.ErrorMessage = string.Empty;
        }
        public LibraryLoaderWorkItem(int id, string fileName, LibraryLoadType loadType)
        {
            this.Id = id;
            this.FileName = fileName;
            this.LoadType = loadType;
            this.LoadState = LibraryWorkItemState.Pending;
            this.ErrorMessage = string.Empty;
        }
        public LibraryLoaderWorkItem(LibraryLoaderWorkItem copy)
        {
            this.Id = copy.Id;
            this.FileName = copy.FileName;
            this.LoadType = copy.LoadType;
            this.LoadState = copy.LoadState;
            this.ErrorMessage = copy.ErrorMessage;
        }

        public void Set(LibraryWorkItemState state, string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.LoadState = state;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            var item = (LibraryLoaderWorkItem)obj;

            return item.LoadState == this.LoadState &&
                    item.ErrorMessage == this.ErrorMessage &&
                    item.FileName == this.FileName &&
                    item.LoadType == this.LoadType &&
                    item.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Id, this.FileName, this.LoadType, this.LoadState, this.ErrorMessage);
        }
    }
}
