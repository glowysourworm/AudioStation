using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component
{
    [IocExport(typeof(IViewModelLoader))]
    public class ViewModelLoader : IViewModelLoader
    {
        readonly IModelController _modelController;

        [IocImportingConstructor]
        public ViewModelLoader(IModelController modelController)
        {
            _modelController = modelController;
        }

        public PageResult<ArtistViewModel> LoadArtistPage(PageRequest<Mp3FileReferenceArtist, string> request)
        {
            var result = new PageResult<ArtistViewModel>();
            var resultCollection = new List<ArtistViewModel>();

            // Database:  Load the artist entities
            var artistPage = _modelController.GetPage(request);

            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.TotalRecordCountFiltered = artistPage.TotalRecordCountFiltered;
            result.TotalRecordCount = artistPage.TotalRecordCount;

            // Load the album collection
            foreach (var artist in artistPage.Results)
            {
                // Database:  Load the album entities
                var albums = _modelController.GetArtistAlbums(artist.Id, true);

                // Create Artist Result
                var artistViewModel = new ArtistViewModel(artist.Id)
                {
                    Artist = artist.Name                    
                };

                // Add Album - Query Tracks
                foreach (var album in albums)
                {
                    var albumViewModel = new AlbumViewModel(album.Id)
                    {
                        Album = album.Name,
                        PrimaryArtist = artist.Name,
                        Year = (uint)album.Year
                    };

                    // Database:  Load the track entities
                    var tracks = _modelController.GetAlbumTracks(album.Id);

                    // Create tracks for the album
                    albumViewModel.Tracks.AddRange(tracks.Select(track =>
                    {
                        return new LibraryEntryViewModel(track.Id)
                        {
                            Album = album.Name,
                            Disc = (uint)album.DiscNumber,
                            FileName = track.FileName,
                            PrimaryArtist = artist.Name,
                            Title = track.Title ?? "Unknown",
                            Track = (uint)(track.Track ?? 0),
                            Duration = TimeSpan.FromMilliseconds(track.DurationMilliseconds ?? 0),
                            FileCorruptMessage = track.FileCorruptMessage ?? "",
                            FileLoadErrorMessage = track.FileErrorMessage ?? "",
                            IsFileAvailable = track.IsFileAvailable,
                            IsFileCorrupt = track.IsFileCorrupt,
                            IsFileLoadError = track.IsFileLoadError,
                            PrimaryGenre = track.PrimaryGenre?.Name ?? "Unknown"
                        };
                    }));

                    // Calculate the album duration
                    albumViewModel.Duration = TimeSpan.FromMilliseconds(albumViewModel.Tracks.Sum(track => track.Duration.TotalMilliseconds));

                    artistViewModel.Albums.Add(albumViewModel);
                }

                // Add Artist to result page
                resultCollection.Add(artistViewModel);
            }

            result.Results = resultCollection;

            return result;
        }

        public PageResult<LibraryEntryViewModel> LoadEntryPage(PageRequest<Mp3FileReference, int> request)
        {
            var result = new PageResult<LibraryEntryViewModel>();

            // Database:  Load the file (entry) entities
            var entryPage = _modelController.GetPage(request);

            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.TotalRecordCountFiltered = entryPage.TotalRecordCountFiltered;
            result.TotalRecordCount = entryPage.TotalRecordCount;
            result.Results = entryPage.Results.Select(entry => new LibraryEntryViewModel(entry.Id)
            {
                Album = entry.Album?.Name ?? "Unknown",
                Disc = (uint)(entry.Album?.DiscNumber ?? 0),
                Duration = TimeSpan.FromMilliseconds(entry.DurationMilliseconds ?? 0),
                FileName = entry.FileName,
                PrimaryArtist = entry.PrimaryArtist?.Name ?? "Unknown",
                PrimaryGenre = entry.PrimaryGenre?.Name ?? "Unknown",
                Title = entry.Title ?? "Unknown",
                Track = (uint)(entry.Track ?? 0),
                FileCorruptMessage = entry.FileCorruptMessage ?? "",
                FileLoadErrorMessage = entry.FileErrorMessage ?? "",
                IsFileAvailable = entry.IsFileAvailable,
                IsFileLoadError = entry.IsFileLoadError,
                IsFileCorrupt = entry.IsFileCorrupt
            }).ToList();

            return result;
        }
    }
}
