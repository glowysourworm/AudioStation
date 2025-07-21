using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelValidationService
    {
        bool ValidateTagImport(TagLib.File tagFile);
        bool ValidateMusicBrainzRecordingImport(IRecording recording);
    }
}
