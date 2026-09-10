using SimpleWpf.UI.ViewModel;

namespace AudioStation.Event.DialogEvents
{
    public class DialogSingleTextFieldViewModel : ViewModelBase
    {
        string _name;
        string _value;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }


        public DialogSingleTextFieldViewModel()
        {
            this.Value = string.Empty;
        }
    }
}
