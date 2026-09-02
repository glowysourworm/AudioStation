using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.Vendor.AcoustIDViewModel
{
    public class AcoustIDLookupResultViewModel : ViewModelBase
    {
        Guid _id;
        double _score;
        string _fingerprint;
        Guid _musicBrainzRecordingId;

        /// <summary>
        /// AcoustID's GUID record
        /// </summary>
        public Guid Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public double Score
        {
            get { return _score; }
            set { this.RaiseAndSetIfChanged(ref _score, value); }
        }
        public string Fingerprint
        {
            get { return _fingerprint; }
            set { this.RaiseAndSetIfChanged(ref _fingerprint, value); }
        }
        public Guid MusicBrainzRecordingId
        {
            get { return _musicBrainzRecordingId; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingId, value); }
        }

        public AcoustIDLookupResultViewModel()
        {
            this.Id = Guid.Empty;
            this.Fingerprint = string.Empty;
            this.MusicBrainzRecordingId = Guid.Empty;
        }

        public override string ToString()
        {
            return string.Format("Score({1:P2})", this.Id, this.Score);
        }
    }
}
