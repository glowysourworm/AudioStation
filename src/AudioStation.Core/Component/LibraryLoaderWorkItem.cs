using System.Diagnostics.CodeAnalysis;

using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Core.Component
{
    public enum LibraryLoadType
    {
        Mp3File,
        M3UFile
    }
    public enum LibraryWorkItemState
    {
        Pending = 0,
        Processing = 1,
        CompleteSuccessful = 2,
        CompleteError = 3
    }
    public struct LibraryLoaderWorkItem
    {
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
        public LibraryLoadType LoadType { get; set; }
        public LibraryWorkItemState LoadState { get; set; }

        // Default constructor used for .Equals comparison / FirstOrDefault / etc...
        public LibraryLoaderWorkItem()
        {
            this.FileName = string.Empty;
            this.LoadType = LibraryLoadType.Mp3File;
            this.LoadState = LibraryWorkItemState.Pending;
            this.ErrorMessage = string.Empty;
        }
        public LibraryLoaderWorkItem(string fileName, LibraryLoadType loadType)
        {
            this.FileName = fileName;
            this.LoadType = loadType;
            this.LoadState = LibraryWorkItemState.Pending;
            this.ErrorMessage = string.Empty;
        }
        public LibraryLoaderWorkItem(LibraryLoaderWorkItem copy)
        {
            this.FileName = copy.FileName;
            this.LoadType = copy.LoadType;
            this.LoadState = copy.LoadState;
            this.ErrorMessage = copy.ErrorMessage;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) 
                return false;

            var item = (LibraryLoaderWorkItem)obj;

            return item.LoadState == this.LoadState &&
                    item.ErrorMessage == this.ErrorMessage &&
                    item.FileName == this.FileName &&
                    item.LoadType == this.LoadType;
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.FileName, this.LoadType, this.LoadState, this.ErrorMessage);
        }
    }
}
