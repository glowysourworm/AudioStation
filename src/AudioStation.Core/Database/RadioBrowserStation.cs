using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("RadioBrowserStation", Schema = "public")]
    public class RadioBrowserStation : EntityBase
    {
        public Guid StationUUID { get; set; }
        public Guid ServerUUID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string UrlResolved { get; set; }
        public string Homepage { get; set; }
        public string Favicon { get; set; }
        public string Tags { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Language { get; set; }
        public string LanguageCodes { get; set; }
        public string Codec { get; set; }
        public int Bitrate { get; set; }
        public int Hls { get; set; }
        public bool UserExcluded { get; set; }

        public RadioBrowserStation() { }
    }
}
