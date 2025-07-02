using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Model
{
    public enum LibraryEntityType
    {
        Track,
        Album,
        Artist,
        Genre
    }

    /// <summary>
    /// TODO: This has to be better integrated. What other types are there?
    /// </summary>
    public enum LibraryEntryType
    {
        Music,
        AudioBook
    }
}
