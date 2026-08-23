namespace AudioStation.Core.Model
{
    public enum LibraryEntryType
    {
        Track,
        Album,
        Artist,
        Genre
    }

    /// <summary>
    /// TODO: This has to be better integrated. What other types are there?
    /// </summary>
    public enum TrackType
    {
        Any = 0,
        Music = 1,
        AudioBook = 2
    }
}
