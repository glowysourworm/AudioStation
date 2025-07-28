using AudioStation.ViewModels.Vendor.ATLViewModel;

using SimpleWpf.ViewModel;

namespace AudioStation.Event.DialogEvents
{
    public class DialogTagFieldEditorViewModel : ViewModelBase
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
    }
}
