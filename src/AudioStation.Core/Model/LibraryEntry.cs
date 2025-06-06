using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// This "LibraryEntry" essentially represents one MP3 file - with database backup of just the fields
    /// we'll be looking at first. Other data will be loaded at runtime. The Library will contain sets for
    /// other lists:  Albums, Artists, and Genres.
    /// </summary>
    public class LibraryEntry
    {
        bool _fileRead = false;

        /// <summary>
        /// This will return true once the library entry's Mp3 file is read
        /// </summary>
        public bool GetFileRead()
        {
            return _fileRead;
        }

        public void SetTagData(TagLib.File tagRef)
        {
            this.PrimaryArtist = tagRef.Tag.FirstAlbumArtist;
            this.PrimaryGenre = tagRef.Tag.FirstGenre;
            this.Album = tagRef.Tag.Album;
            this.Title = tagRef.Tag.Title;
            this.Track = tagRef.Tag.Track;
            this.Disc = tagRef.Tag.Disc;

            // Could've been exceptions thrown during loading. These are more important than the 
            // taglib errors.
            //
            this.FileError = this.FileError || tagRef.PossiblyCorrupt;
            this.FileErrorMessage = this.FileErrorMessage ?? tagRef.CorruptionReasons.Join(",", x => x);

            _fileRead = true;
        }

        /// <summary>
        /// Id (from database) of this entry
        /// </summary>
        public int Id { get; set; }
        public string FileName { get; set; }
        public string PrimaryArtist { get; set; }
        public string PrimaryGenre { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public uint Track { get; set; }
        public uint Disc { get; set; }

        public bool FileError { get; set; }
        public string FileErrorMessage { get; set; }

        public LibraryEntry()
        {
            this.FileName = string.Empty;
            this.PrimaryGenre = string.Empty;
            this.PrimaryArtist = string.Empty;
            this.Title = string.Empty;
            this.Album = string.Empty;
            this.FileErrorMessage = string.Empty;
        }
    }
}
