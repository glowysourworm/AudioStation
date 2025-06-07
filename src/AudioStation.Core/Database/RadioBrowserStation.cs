using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database
{
    [Table("RadioBrowserStation", Schema = "public")]
    public class RadioBrowserStation
    {
        public int Id { get; set; }
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
        public RadioBrowserStation(int Id_, Guid StationUUID_, Guid ServerUUID_, string Name_, string Url_, string UrlResolved_, string Homepage_, string Favicon_, string Tags_, string Country_, string State_, string Language_, string LanguageCodes_, string Codec_, int Bitrate_, int Hls_, bool UserExcluded_)
        {
            this.Id = Id_;
            this.StationUUID = StationUUID_;
            this.ServerUUID = ServerUUID_;
            this.Name = Name_;
            this.Url = Url_;
            this.UrlResolved = UrlResolved_;
            this.Homepage = Homepage_;
            this.Favicon = Favicon_;
            this.Tags = Tags_;
            this.Country = Country_;
            this.State = State_;
            this.Language = Language_;
            this.LanguageCodes = LanguageCodes_;
            this.Codec = Codec_;
            this.Bitrate = Bitrate_;
            this.Hls = Hls_;
            this.UserExcluded = UserExcluded_;
        }
    }
}
