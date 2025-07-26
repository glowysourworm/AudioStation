using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

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

        public bool ValidateTagImport(IAudioStationTag simpleTag, out string validationMessage)
        {
            return ValidateImport(simpleTag.AlbumArtist,
                                  simpleTag.Album,
                                  simpleTag.Title, 
                                  simpleTag.Genre,
                                  simpleTag.Track,
                                  simpleTag.TrackTotal,
                                  simpleTag.DiscNumber,
                                  simpleTag.DiscTotal, out validationMessage);
        }

        private bool ValidateImport(string firstAlbumArtist,
                                    string album,
                                    string title,
                                    string genre,
                                    uint trackNumber,
                                    uint trackCount,
                                    uint discNumber,
                                    uint discCount,
                                    out string validationMessage)
        {
            // Validated Fields
            var valid = true;

            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(firstAlbumArtist))
            {
                valid = false;
                validationMessage += "(Album Artist)";
            }


            if (string.IsNullOrWhiteSpace(album))
            {
                valid = false;
                validationMessage += "(Album)";
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                valid = false;
                validationMessage += "(Title)";
            }

            if (string.IsNullOrWhiteSpace(genre))
            {
                valid = false;
                validationMessage += "(Genre)";
            }

            if (trackNumber <= 0 || trackNumber > trackCount)
            {
                valid = false;
                validationMessage += "(Track Number)";
            }

            if (trackCount <= 0 || trackCount < trackNumber)
            {
                valid = false;
                validationMessage += "(Track Count)";
            }

            if (discNumber <= 0)
            {
                valid = false;
                validationMessage += "(Disc)";
            }

            if (discCount <= 0)
            {
                valid = false;
                validationMessage += "(Disc Count)";
            }

            return valid;
        }
    }
}
