using System.Collections.Generic;

using AudioStation.Model.Vendor;

namespace AudioStation.Model.Comparer
{
    public class MusicBrainzScoreComparer : Comparer<MusicBrainzRecord>
    {
        public override int Compare(MusicBrainzRecord x, MusicBrainzRecord y)
        {
            return x.Score.CompareTo(y.Score);
        }
    }
}
