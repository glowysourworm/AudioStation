namespace AudioStation.Core.Model
{
    public class RadioEntry
    {
        public string Name { get; set; }
        public List<RadioEntryStreamInfo> Streams { get; set; }

        // These only apply to file type entries
        public string FileName { get; set; }
        public bool FileError { get; set; }
        public string FileErrorMessage { get; set; }

        public RadioEntry()
        {
            this.Name = string.Empty;
            this.Streams = new List<RadioEntryStreamInfo>();
            this.FileErrorMessage = string.Empty;
        }
    }
}
