using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;

namespace AudioStation.Core.Model
{
    public class LibraryFormat : ILibraryFormat
    {
        List<ILibraryFormatField> _fields;
        LibraryFormatUse _use;

        public IEnumerable<ILibraryFormatField> Fields
        {
            get { return _fields; }
            set
            {
                _fields.Clear();
                foreach (var item in value)
                    _fields.Add(item);
            }
        }
        public LibraryFormatUse Use
        {
            get { return _use; }
            set { _use = value; }
        }

        public string LeftToken { get; set; }
        public string RightToken { get; set; }

        public LibraryFormat()
        {
            this.LeftToken = MigrationHelpers.FormatLeftToken;
            this.RightToken = MigrationHelpers.FormatRightToken;

            _fields = new List<ILibraryFormatField>();
            _use = LibraryFormatUse.File;
        }
    }
}
