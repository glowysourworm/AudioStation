namespace AudioStation.Core.Model
{
    public class RadioEntry
    {
        public string Name { get; set; }
        public List<RadioEntryStreamInfo> Streams { get; set; }

        public RadioEntry()
        {
            this.Name = string.Empty;
            this.Streams = new List<RadioEntryStreamInfo>();
        }
    }
}
