using AudioStation.Core.Model.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelValidationService
    {
        bool ValidateTagImport(ISimpleTag tag, out string validationMessage);
        bool ValidateTagImport(TagLib.File tagFile, out string validationMessage);
        bool ValidateMusicBrainzRecordingImport(IRecording recording);
    }
}
