using AudioStation.Controls;

using SimpleWpf.UI.ViewModel;

using Status = AudioStation.Core.Service.Interface.IAudioStationDataService.Status;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class StatusIconViewModel : ViewModelBase
    {
        string _message;
        Status _status;
        AudioStationIconState _iconState;
        int _activityBadgeCount;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public Status Status
        {
            get { return _status; }
            set
            {
                if (this.RaiseAndSetIfChanged(ref _status, value))
                    UpdateIconState();
            }
        }
        public AudioStationIconState IconState
        {
            get { return _iconState; }
            private set { this.RaiseAndSetIfChanged(ref _iconState, value); }
        }
        public int ActivityBadgeCount
        {
            get { return _activityBadgeCount; }
            set { this.RaiseAndSetIfChanged(ref _activityBadgeCount, value); }
        }


        public StatusIconViewModel()
        {
            this.Message = string.Empty;
            this.Status = Status.Disabled;
            this.IconState = AudioStationIconState.Disabled;
        }

        private void UpdateIconState()
        {
            switch (_status)
            {
                case Status.Disabled:
                    this.IconState = AudioStationIconState.Disabled;
                    break;
                case Status.Idle:
                    this.IconState = AudioStationIconState.Enabled;
                    break;
                case Status.Working:
                    this.IconState = AudioStationIconState.Active;
                    break;
                case Status.Error:
                    this.IconState = AudioStationIconState.Error;
                    break;
                default:
                    throw new Exception("Unhandled Icon Status");
            }
        }
    }
}
