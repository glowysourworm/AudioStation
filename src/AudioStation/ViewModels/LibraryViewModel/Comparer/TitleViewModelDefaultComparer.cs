using System.Collections.Generic;

using AudioStation.ViewModel.LibraryViewModel;

namespace AudioStation.ViewModels.LibraryViewModel.Comparer
{
    public class TitleViewModelDefaultComparer : Comparer<TitleViewModel>
    {
        public override int Compare(TitleViewModel? x, TitleViewModel? y)
        {
            if (x == null && y == null)
                return 0;

            else if (x == null)
                return -1;

            else if (y == null)
                return 1;

            else
                return x.Track.CompareTo(y.Track);
        }
    }
}
