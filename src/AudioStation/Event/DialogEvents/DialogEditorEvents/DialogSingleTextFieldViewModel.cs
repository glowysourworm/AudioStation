namespace AudioStation.Event.DialogEvents.DialogEditorEvents
{
    public class DialogSingleTextFieldViewModel : DialogEditorViewModelBase
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

        protected override bool Validate(out bool isComplete, out string validationMessage)
        {
            var valid = !string.IsNullOrWhiteSpace(this.Value);

            isComplete = valid;
            validationMessage = valid ? string.Empty : "Please enter a value";

            return valid;
        }
    }
}
