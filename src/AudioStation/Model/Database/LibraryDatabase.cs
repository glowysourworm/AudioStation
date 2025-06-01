using System;
using System.Collections.Generic;

using AudioStation.Component;

using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model.Database
{
    /// <summary>
    /// Represents library entries and other serialized data for the library. This is separate to
    /// the Configuration (serialized) object.
    /// </summary>
    public class LibraryDatabase : IRecursiveSerializable
    {
        public List<LibraryEntry> Entries { get; set; }

        public LibraryDatabase()
        {
            this.Entries = new List<LibraryEntry>();
        }
        public LibraryDatabase(Library library)
        {
            this.Entries = new List<LibraryEntry>(library.AllTitles);
        }
        public LibraryDatabase(IEnumerable<LibraryEntry> entries)
        {
            this.Entries = new List<LibraryEntry>(entries);
        }
        public LibraryDatabase(IPropertyReader reader)
        {
            this.Entries = reader.Read<List<LibraryEntry>>("Entries");
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Entries", this.Entries);
        }

        /// <summary>
        /// Creates library object from loaded data
        /// </summary>
        public Library CreateLibrary()
        {
            var library = new Library();

            foreach (var entry in this.Entries)
                library.Add(entry);

            return library;
        }

        public static LibraryDatabase Open(string fileName)
        {
            try
            {
                return Serializer.Deserialize<LibraryDatabase>(fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the database to file. Returns success / failure.
        /// </summary>
        public bool Save(string fileName)
        {
            try
            {
                Serializer.Serialize(this, fileName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
