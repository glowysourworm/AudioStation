using System.Collections.Generic;

using AudioStation.ViewModel.LibraryViewModel;

namespace AudioStation.Model.Comparer
{
    public class TrackNumberComparer : Comparer<TitleViewModel>
    {
        public override int Compare(TitleViewModel x, TitleViewModel y)
        {
            return x.Track.CompareTo(y.Track);
        }
    }
}
