using AudioStation.Core.Model.Interface;

using CSCore;

namespace AudioStation.Core.Model
{
    public class LibraryDirectory : ILibraryDirectory
    {
        AudioEncoderInfo _audioEncoderInfo;

        public string DirectoryLabel { get; set; }
        public string Directory { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsReadOnly { get; set; }
        public bool DeleteUnusedFolders { get; set; }
        public AudioEncoderInfo FormatPreference
        {
            get { return _audioEncoderInfo; }
            set { _audioEncoderInfo = value; }
        }
        public TrackCategory TrackCategory { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
        public LibraryImportType ImportType { get; set; }

        IAudioEncoderInfo ILibraryDirectory.FormatPreference
        {
            get { return _audioEncoderInfo; }
            set
            {
                _audioEncoderInfo = value as AudioEncoderInfo;
            }
        }

        public LibraryDirectory()
        {
            this.DirectoryLabel = string.Empty;
            this.Directory = string.Empty;
            this.TrackCategory = TrackCategory.Any;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;
            this.ImportType = LibraryImportType.InPlaceDirectory;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.FormatPreference = new AudioEncoderInfo()
            {
                Encoding = AudioEncoding.MpegLayer3,
                Extension = ".mp3",
                Filter = "*.mp3",
                Name = "Mpeg Layer 3"
            };
        }
    }
}
