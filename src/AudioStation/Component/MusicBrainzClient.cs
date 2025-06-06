using AudioStation.Core.Model;
using AudioStation.ViewModels.Vendor;

using MetaBrainz.MusicBrainz;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Component
{
    public static class MusicBrainzClient
    {
        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        public static Task<IEnumerable<MusicBrainzRecordViewModel>> Query(LibraryEntry entry)
        {
            return Task.Run<IEnumerable<MusicBrainzRecordViewModel>>(() =>
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();

                return query.FindRecordings(string.Format("title:{0} artist:{1} release:{2}", entry.Title, entry.PrimaryArtist, entry.Album))
                            .Results
                            .Select(result => new
                            {
                                Id = result.Item.Id,
                                Title = result.Item.Title,
                                Releases = result.Item.Releases,
                                Artists = result.Item.ArtistCredit,
                                Score = result.Score
                            })
                            .SelectMany(x => x.Releases.Select(release =>
                            {
                                return new
                                {
                                    Id = x.Id,
                                    Title = x.Title,
                                    Release = release.Title,
                                    Track = release.Media?.FirstOrDefault()?.Position ?? -1,
                                    Country = release.Country,
                                    Status = release.Status,
                                    MediumList = release.Media,
                                    Credits = release.ArtistCredit,
                                    ReleaseEvents = release.ReleaseEvents,
                                    ReleaseGroup = release.ReleaseGroup,
                                    Score = x.Score,
                                    Year = release.Date?.Year,
                                    Artists = x.Artists.Select(artist => artist.Artist.SortName)
                                };
                            }))
                              .Select(x => new MusicBrainzRecordViewModel(x.Id.ToString())
                              {
                                  Album = x.Release,
                                  AlbumArtists = new SortedObservableCollection<string>(x.Artists),
                                  MusicBrainzReleaseCountry = x.Country,
                                  MusicBrainzReleaseStatus = x.Status,
                                  Title = x.Title,
                                  Track = (uint)x.Track,
                                  Score = x.Score,
                                  Year = (uint)x.Year
                              })
                              .OrderBy(x => x.Title)
                              .ToList();
            });
        }
    }
}
