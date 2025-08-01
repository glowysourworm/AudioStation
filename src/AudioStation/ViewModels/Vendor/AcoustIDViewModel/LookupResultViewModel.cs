﻿using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.AcoustIDViewModel
{
    public class LookupResultViewModel : ViewModelBase
    {
        Guid _id;
        double _score;
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
        public Guid MusicBrainzRecordingId
        {
            get { return _musicBrainzRecordingId; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingId, value); }
        }

        public LookupResultViewModel()
        {

        }

        public override string ToString()
        {
            return string.Format("Score({1:P2})", this.Id, this.Score);
        }
    }
}
