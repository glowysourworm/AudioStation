namespace AudioStation.Core.Model.Interface
{
    /// <summary>
    /// Format field: Formatting is done by Audio Station utilities. The format data is taken from the target object's property and
    /// applied to the string.
    /// </summary>
    public interface ILibraryFormatField
    {
        /// <summary>
        /// Name of the format field:  (e.g. Genre)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the format field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Property name of the target object property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// This is the token string without any surrounding token characters
        /// </summary>
        public string TokenName { get; set; }

        /// <summary>
        /// Field format does not originate from a target object. (e.g. file extension)
        /// </summary>
        public bool IsExtraneous { get; set; }

        /// <summary>
        /// Type of extraneous field
        /// </summary>
        public LibraryExtraneousFields ExtraneousType { get; set; }
    }
}
