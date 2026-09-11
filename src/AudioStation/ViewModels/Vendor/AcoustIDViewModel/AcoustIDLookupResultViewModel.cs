using AudioStation.Core.Database.AudioStationDatabase.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.Vendor.AcoustIDViewModel
{
    public class AcoustIDLookupResultViewModel : ViewModelBase, IAcoustIDLookupResult
    {
        int _id;
        string _fileName;
        Guid _lookupId;
        Guid _musicBrainzRecordingId;
        double _score;
        string _fingerprint;
        int? _importWorkflowId;

        /// <summary>
        /// Database reference ID
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        /// <summary>
        /// AcoustID's lookup record
        /// </summary>
        public Guid LookupId
        {
            get { return _lookupId; }
            set { this.RaiseAndSetIfChanged(ref _lookupId, value); }
        }
        public Guid MusicBrainzRecordingId
        {
            get { return _musicBrainzRecordingId; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingId, value); }
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
        public int? ImportWorkflowId
        {
            get { return _importWorkflowId; }
            set { this.RaiseAndSetIfChanged(ref _importWorkflowId, value); }
        }


        public AcoustIDLookupResultViewModel()
        {
            this.FileName = string.Empty;
            this.Fingerprint = string.Empty;
            this.LookupId = Guid.Empty;
            this.Fingerprint = string.Empty;
            this.MusicBrainzRecordingId = Guid.Empty;
            this.ImportWorkflowId = null;
        }

        public override string ToString()
        {
            return string.Format("Score({1:P2})", this.Id, this.Score);
        }
    }
}
