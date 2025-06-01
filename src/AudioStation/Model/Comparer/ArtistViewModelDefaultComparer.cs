using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.ViewModel.LibraryViewModel;

namespace AudioStation.Model.Comparer
{
    public class ArtistViewModelDefaultComparer : Comparer<ArtistViewModel>
    {
        public override int Compare(ArtistViewModel? x, ArtistViewModel? y)
        {
            if (x == null && y == null)
                return 0;

            else if (x == null)
                return -1;

            else if (y == null)
                return 1;

            else
                return x.Artist.CompareTo(y.Artist);
        }
    }
}
