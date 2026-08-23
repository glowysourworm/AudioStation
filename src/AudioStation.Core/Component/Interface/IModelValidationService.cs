using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelValidationService
    {
        ITagSmallValidation ValidateTagImport(IAudioStationTag tagFile);
        bool ValidateMusicBrainzRecordingImport(IRecording recording);
    }
}
