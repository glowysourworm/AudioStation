using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Utility
{
    public static class TagUtility
    {
        public static string CreateDropdownText(ITagSmall tagSmall)
        {
            var header = CreateDropdownHeader(tagSmall.Title, tagSmall.Album, tagSmall.AlbumArtist);
            var footer = CreateDropdownFooter(tagSmall.TrackNumber, tagSmall.TrackTotal);

            return header + "\t" + footer;
        }

        public static string CreateDropdownText(MusicBrainzAcoustIDResult result)
        {
            var header = CreateDropdownHeader(result.Title, result.Album, result.AlbumArtist);
            var footer = CreateDropdownFooter(result.TrackNumber, result.TrackTotal, result.Score);

            return header + "\t" + footer;
        }

        private static string CreateDropdownHeader(string? title, string? album, string? artist)
        {
            return title ?? album ?? artist ?? "(Not Set)";
        }
        private static string CreateDropdownFooter(int? trackNumber, int? trackTotal, double? score = null)
        {
            var format = "[{0} of {1}]";
            var result = string.Format(format, trackNumber ?? 0, trackTotal ?? 0);

            if (score != null)
                result += string.Format(" ({0:P2})", score);

            return result;
        }
    }
}
