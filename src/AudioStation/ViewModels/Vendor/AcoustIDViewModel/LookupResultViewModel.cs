﻿using SimpleWpf.Extensions;

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
    }
}
