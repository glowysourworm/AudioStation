using System.Collections.Generic;

namespace AudioStation.Model.Comparer
{
    internal class LibraryEntryDefaultComparer : Comparer<LibraryEntry>
    {
        public override int Compare(LibraryEntry? x, LibraryEntry? y)
        {
            if (x == null && y == null)
                return 0;

            else if (x == null)
                return -1;

            else if (y == null)
                return 1;

            else
                return x.FileName.CompareTo(y.FileName);
        }
    }
}
