using AudioStation.ViewModels.Vendor.ATLViewModel;

namespace AudioStation.Event.DialogEvents.DialogEditorEvents
{
    public class DialogTagFieldEditorViewModel : DialogEditorViewModelBase
    {
        string _tagFieldName;
        TagViewModel _tag;

        public string TagFieldName
        {
            get { return _tagFieldName; }
            set { this.RaiseAndSetIfChanged(ref _tagFieldName, value); }
        }
        public TagViewModel Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }

        public DialogTagFieldEditorViewModel()
        {
            this.Tag = new TagViewModel();
        }

        protected override bool Validate(out bool isComplete, out string validationMessage)
        {
            isComplete = true;
            validationMessage = string.Empty;
            return true;
        }
    }
}
