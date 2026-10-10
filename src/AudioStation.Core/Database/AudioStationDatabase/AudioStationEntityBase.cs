namespace AudioStation.Core.Database.AudioStationDatabase
{
    public class AudioStationEntityBase
    {
        public Guid Id { get; set; }

        public AudioStationEntityBase()
        {
            this.Id = Guid.Empty;
        }
    }

    public class AudioStationEnumEntityBase : AudioStationEntityBase
    {
        public string Name { get; set; }

        public AudioStationEnumEntityBase()
        {
            this.Name = string.Empty;
        }
    }
}
