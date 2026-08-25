using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions.Collection;
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

        public ITagSmallValidation ValidateTagImport(IAudioStationTag simpleTag)
        {
            return ValidateImport(simpleTag.AlbumArtist,
                                  simpleTag.Album,
                                  simpleTag.Title,
                                  simpleTag.Genre,
                                  (int)simpleTag.Track,
                                  simpleTag.TrackTotal,
                                  simpleTag.DiscNumber,
                                  simpleTag.DiscTotal,
                                  simpleTag.MediaFormat,
                                  (int)simpleTag.Duration.TotalMilliseconds,
                                  simpleTag.Year);
        }

        public ITagSmallValidation ValidateTagSmallImport(ITagSmall tagSmall)
        {
            return ValidateImport(tagSmall.AlbumArtist,
                                  tagSmall.Album,
                                  tagSmall.Title,
                                  tagSmall.Genre,
                                  tagSmall.TrackNumber,
                                  tagSmall.TrackTotal,
                                  tagSmall.MediaNumber,
                                  tagSmall.MediaTotal,
                                  tagSmall.MediaFormat,
                                  tagSmall.DurationMilliseconds,
                                  tagSmall.Year);
        }

        private ITagSmallValidation ValidateImport(string? albumArtist,
                                                   string? album,
                                                   string? title,
                                                   string? genre,
                                                   int? trackNumber,
                                                   int? trackCount,
                                                   int? mediaNumber,
                                                   int? mediaCount,
                                                   string? mediaFormat,
                                                   int? durationMilliseconds,
                                                   int? year)
        {
            var validation = new TagValidation();
            var invalidFields = new List<string>();

            validation.IsAlbumArtistValid = true;
            validation.IsAlbumValid = true;
            validation.IsTitleValid = true;
            validation.IsGenreValid = true;
            validation.IsTrackValid = true;
            validation.IsTrackTotalValid = true;
            validation.IsMediaNumberValid = true;
            validation.IsMediaTotalValid = true;
            validation.IsMediaFormatValid = true;
            validation.IsDurationMillisecondsValid = true;
            validation.IsYearValid = true;

            if (string.IsNullOrWhiteSpace(albumArtist))
            {
                invalidFields.Add("Album Artist");

                validation.IsAlbumArtistValid = false;
            }


            if (string.IsNullOrWhiteSpace(album))
            {
                invalidFields.Add("Album");

                validation.IsAlbumValid = false;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                invalidFields.Add("Title");

                validation.IsTitleValid = false;
            }

            if (string.IsNullOrWhiteSpace(genre))
            {
                invalidFields.Add("Genre");

                validation.IsGenreValid = false;
            }

            if (trackNumber <= 0 || trackNumber > trackCount)
            {
                invalidFields.Add("Track Number");

                validation.IsTrackValid = false;
            }

            if (trackCount <= 0 || trackCount < trackNumber)
            {
                invalidFields.Add("Track Count");

                validation.IsTrackTotalValid = false;
            }

            if (mediaNumber <= 0)
            {
                invalidFields.Add("Media Number");

                validation.IsMediaNumberValid = false;
            }

            if (mediaCount <= 0)
            {
                invalidFields.Add("Media Count");

                validation.IsMediaTotalValid = false;
            }

            if (string.IsNullOrWhiteSpace(mediaFormat))
            {
                invalidFields.Add("Media Format");

                validation.IsMediaFormatValid = false;
            }
            if (durationMilliseconds <= 0)
            {
                invalidFields.Add("Duration");

                validation.IsDurationMillisecondsValid = false;
            }
            if (year <= 0)
            {
                invalidFields.Add("Year");

                validation.IsYearValid = false;
            }

            validation.ValidationMessage = invalidFields.Join(",", x => x);
            validation.IsValid = invalidFields.Count == 0;

            return validation;
        }
    }
}
