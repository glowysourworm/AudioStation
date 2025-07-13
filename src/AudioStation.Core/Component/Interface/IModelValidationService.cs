using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Interface
{
    public interface IModelValidationService
    {
        bool ValidateMusicBrainzRecording_ImportBasic(IRecording recording);
    }
}
