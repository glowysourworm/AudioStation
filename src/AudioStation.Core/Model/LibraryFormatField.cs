using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    public class LibraryFormatField : ILibraryFormatField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PropertyName { get; set; }
        public LibraryFormatUse Use { get; set; }
        public bool IsExtraneous { get; set; }
        public string TokenName { get; set; }
        public LibraryExtraneousFields ExtraneousType { get; set; }

        public LibraryFormatField()
        {
            this.Name = string.Empty;
            this.PropertyName = string.Empty;
            this.TokenName = string.Empty;
        }
    }
}
