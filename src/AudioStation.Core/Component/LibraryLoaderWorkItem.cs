using System.Diagnostics.CodeAnalysis;

using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Core.Component
{
    public enum LibraryLoadType
    {
        Mp3File,
        M3UFile
    }
    public struct LibraryLoaderWorkItem
    {
        public string FileName { get; set; }
        public LibraryLoadType LoadType { get; set; }
        public bool ProcessingSuccessful { get; set; }
        public bool ProcessingComplete { get; set; }

        // Default constructor used for .Equals comparison / FirstOrDefault / etc...
        public LibraryLoaderWorkItem()
        {
            this.FileName = string.Empty;
            this.LoadType = LibraryLoadType.Mp3File;
            this.ProcessingSuccessful = false;
            this.ProcessingComplete = false;
        }
        public LibraryLoaderWorkItem(string fileName, LibraryLoadType loadType)
        {
            this.FileName = fileName;
            this.LoadType = loadType;
            this.ProcessingSuccessful = false;
            this.ProcessingComplete = false;
        }
        public LibraryLoaderWorkItem(LibraryLoaderWorkItem copy)
        {
            this.FileName = copy.FileName;
            this.LoadType = copy.LoadType;
            this.ProcessingSuccessful = copy.ProcessingSuccessful;
            this.ProcessingComplete = copy.ProcessingComplete;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) 
                return false;

            var item = (LibraryLoaderWorkItem)obj;

            return item.ProcessingComplete == this.ProcessingComplete &&
                    item.ProcessingSuccessful == this.ProcessingSuccessful &&
                    item.FileName == this.FileName &&
                    item.LoadType == this.LoadType;
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.FileName, this.LoadType, this.ProcessingComplete, this.ProcessingSuccessful);
        }
    }
}
