using System.Collections.ObjectModel;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LogViewModels
{
    public class LogComponentViewModel : ViewModelBase
    {
        string _name;
        ObservableCollection<LogSubComponentViewModel> _subComponents;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public ObservableCollection<LogSubComponentViewModel> SubComponents
        {
            get { return _subComponents; }
            set { this.RaiseAndSetIfChanged(ref _subComponents, value); }
        }

        public LogComponentViewModel()
        {
            this.Name = string.Empty;
            this.SubComponents = new ObservableCollection<LogSubComponentViewModel>();
        }
    }
}
