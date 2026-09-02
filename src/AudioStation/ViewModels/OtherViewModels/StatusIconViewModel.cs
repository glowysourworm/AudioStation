using SimpleWpf.UI.ViewModel;

using Status = AudioStation.Core.Service.Interface.IAudioStationService.Status;

namespace AudioStation.ViewModels.OtherViewModels
{
    public class StatusIconViewModel : ViewModelBase
    {
        string _message;
        Status _status;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public Status Status
        {
            get { return _status; }
            set { this.RaiseAndSetIfChanged(ref _status, value); }
        }

        public StatusIconViewModel()
        {
            this.Message = string.Empty;
            this.Status = Status.Disabled;
        }
    }
}
