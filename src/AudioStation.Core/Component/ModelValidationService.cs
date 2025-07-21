using AudioStation.Core.Component.Interface;

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

        public bool ValidateTagImport(TagLib.File tagFile)
        {
            // Validated Fields
            var valid = true;

            if (string.IsNullOrWhiteSpace(tagFile.Tag.FirstAlbumArtist))
                valid = false;

            if (string.IsNullOrWhiteSpace(tagFile.Tag.Album))
                valid = false;

            if (string.IsNullOrWhiteSpace(tagFile.Tag.Title))
                valid = false;

            if (tagFile.Tag.Track <= 0)
                valid = false;

            if (tagFile.Tag.TrackCount <= 0)
                valid = false;

            if (tagFile.Tag.Disc <= 0)
                valid = false;

            if (tagFile.Tag.DiscCount <= 0)
                valid = false;

            return valid;
        }
    }
}
