using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelValidationService
    {
        bool ValidateTagImport(IAudioStationTag tagFile, out string validationMessage);
        bool ValidateMusicBrainzRecordingImport(IRecording recording);
    }
}
