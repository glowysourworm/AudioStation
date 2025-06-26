using AudioStation.ViewModels.PlaylistViewModels.Interface;

namespace AudioStation.Component.Model
{
    public class NowPlayingData
    {
        public string ArtistArticle { get; set; }
        public string ArtistSummary { get; set; }
        public string BestImage { get; set; }
        public IEnumerable<string> ExternalLinks { get; set; }
        public IEnumerable<IPlaylistEntryViewModel> Entries { get; set; }
        public IPlaylistEntryViewModel NowPlaying {  get; set; }
    }
}
