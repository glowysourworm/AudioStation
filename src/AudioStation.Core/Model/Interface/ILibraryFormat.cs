namespace AudioStation.Core.Model.Interface
{
    [Flags]
    public enum LibraryFormatUse
    {
        File,
        Folder
    }

    /// <summary>
    /// Library format for use to templatize strings in the application. The most common use is for the
    /// file + folder custom formats.
    /// </summary>
    public interface ILibraryFormat
    {
        /// <summary>
        /// Format usage (Files, Folders, etc...)
        /// </summary>
        public LibraryFormatUse Use { get; set; }

        /// <summary>
        /// Character(s) to surround format fields (on the left)
        /// </summary>
        string LeftToken { get; set; }

        /// <summary>
        /// Character(s) to surround format fields (on the right)
        /// </summary>
        string RightToken { get; set; }

        /// <summary>
        /// Fields from the target object with which to apply the format
        /// </summary>
        IEnumerable<ILibraryFormatField> Fields { get; set; }
    }
}
