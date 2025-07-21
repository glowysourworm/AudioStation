using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IModelValidationService))]
    public class ModelValidationService : IModelValidationService
    {
        [IocImportingConstructor]
        public ModelValidationService()
        {

        }

        public bool ValidateMusicBrainzRecordingImport(IRecording recording)
        {
            return recording != null &&
                   recording.ArtistCredit != null &&
                   recording.ArtistCredit.Any() &&
                   recording.Releases != null &&
                   recording.Releases.Any() &&
                  !string.IsNullOrWhiteSpace(recording.ArtistCredit.First().Name) &&
                  !string.IsNullOrWhiteSpace(recording.Releases.First().Title) &&
                  !string.IsNullOrWhiteSpace(recording.Title) &&
                   recording.Releases.First().Media != null &&
                   recording.Releases.First().Media.Any() &&
                   recording.Releases.First().Media.FirstOrDefault(x => x.Tracks != null) != null &&
                   recording.Releases.First().Media.FirstOrDefault(x => x.Tracks.Any(z => z.Title == recording.Title)) != null &&
                   recording.Releases.First().Media.First(x => x.Tracks.Any(z => z.Title == recording.Title))
                                                   .Tracks
                                                   .First(x => x.Title == recording.Title).Position > 0 &&
                   recording.Releases.First().Media.First(x => x.Tracks.Any(z => z.Title == recording.Title))
                                                                       .TrackCount > 0;
        }

        public bool ValidateTagImport(ISimpleTag simpleTag)
        {
            return ValidateImport(simpleTag.FirstAlbumArtist,
                                  simpleTag.Album,
                                  simpleTag.Title,
                                  simpleTag.Track,
                                  simpleTag.TrackCount,
                                  simpleTag.Disc,
                                  simpleTag.DiscCount);
        }

        public bool ValidateTagImport(TagLib.File tagFile)
        {
            return ValidateImport(tagFile.Tag.FirstAlbumArtist,
                                  tagFile.Tag.Album,
                                  tagFile.Tag.Title,
                                  tagFile.Tag.Track,
                                  tagFile.Tag.TrackCount,
                                  tagFile.Tag.Disc,
                                  tagFile.Tag.DiscCount);
        }

        private bool ValidateImport(string firstAlbumArtist,
                                    string album,
                                    string title,
                                    uint trackNumber,
                                    uint trackCount,
                                    uint discNumber,
                                    uint discCount)
        {
            // Validated Fields
            var valid = true;

            if (string.IsNullOrWhiteSpace(firstAlbumArtist))
                valid = false;

            if (string.IsNullOrWhiteSpace(album))
                valid = false;

            if (string.IsNullOrWhiteSpace(title))
                valid = false;

            if (trackNumber <= 0)
                valid = false;

            if (trackCount <= 0)
                valid = false;

            if (discNumber <= 0)
                valid = false;

            if (discCount <= 0)
                valid = false;

            return valid;
        }
    }
}
