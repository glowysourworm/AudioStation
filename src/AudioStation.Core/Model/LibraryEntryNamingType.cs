namespace AudioStation.Core.Model
{
    public enum LibraryEntryNamingType
    {
        None = 0,

        /// <summary>
        /// Naming follows the track title from the tag and includes the track number
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Naming has artist, album, and track title, including track number, from the 
        /// tag data.
        /// </summary>
        Descriptive = 2
    }
}
