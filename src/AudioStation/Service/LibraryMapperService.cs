using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Service
{
    [IocExport(typeof(ILibraryMapperService))]
    public class LibraryMapperService : ILibraryMapperService
    {
        [IocImportingConstructor]
        public LibraryMapperService()
        {

        }

        public void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        public AlbumViewModel MapAlbum(Artist primaryArtist, Album albumEntity, IEnumerable<Track> tracks)
        {
            return new AlbumViewModel(albumEntity.Id)
            {
                Album = albumEntity.Name,
                Duration = TimeSpan.FromMilliseconds((double)tracks.Sum(track => track.DurationMilliseconds)),
                PrimaryArtist = primaryArtist.Name,
                Tracks = new SortedObservableCollection<TrackViewModel>(tracks.Select(MapTrack)),
                Year = (uint)albumEntity.Year
            };
        }

        public TrackViewModel MapTrack(Track track)
        {
            return new TrackViewModel(track.Id)
            {
                Album = track.Album?.Name ?? "Unknown",
                Disc = (uint)(track.Album?.MediaNumber ?? 0),
                Duration = TimeSpan.FromMilliseconds(track.DurationMilliseconds ?? 0),
                FileName = track.FileReference.FileName,
                PrimaryArtist = track.PrimaryArtist?.Name ?? "Unknown",
                PrimaryGenre = track.PrimaryGenre?.Name ?? "Unknown",
                Title = track.Title ?? "Unknown",
                Track = (uint)(track.Number ?? 0),
                FileCorruptMessage = track.FileReference.FileCorruptMessage ?? "",
                FileLoadErrorMessage = track.FileReference.FileErrorMessage ?? "",
                IsFileAvailable = track.FileReference.IsFileAvailable,
                IsFileLoadError = track.FileReference.IsFileLoadError,
                IsFileCorrupt = track.FileReference.IsFileCorrupt,
                Crc32 = track.FileReference.CRC32
            };
        }
    }
}
