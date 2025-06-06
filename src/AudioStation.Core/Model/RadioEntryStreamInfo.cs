namespace AudioStation.Core.Model
{
    public class RadioEntryStreamInfo
    {
        public string Name { get; set; }
        public string Homepage { get; set; }
        public string Endpoint { get; set; }
        public string LogoEndpoint { get; set; }
        public int Bitrate { get; set; }
        public string Codec { get; set; }

        public RadioEntryStreamInfo()
        {
            this.Name = string.Empty;
            this.Endpoint = string.Empty;
            this.LogoEndpoint = string.Empty;
            this.Homepage = string.Empty;
            this.Codec = string.Empty;
        }
    }
}
