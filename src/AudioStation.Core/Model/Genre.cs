namespace AudioStation.Core.Model
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Genre()
        {
            this.Name = string.Empty;
        }
    }
}
