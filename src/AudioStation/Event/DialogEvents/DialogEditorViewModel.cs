using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Interface;

namespace AudioStation.Event.DialogEvents
{
    public class DialogEditorViewModel : DialogViewModelBase
    {
        ValidationViewModelBase _theObject;
        IValidationContext _validationContext;

        public ValidationViewModelBase TheObject
        {
            get { return _theObject; }
            set { this.RaiseAndSetIfChanged(ref _theObject, value); }
        }

        public IValidationContext ValidationContext
        {
            get { return _validationContext; }
            set { this.RaiseAndSetIfChanged(ref _validationContext, value); }
        }

        protected override bool Validate()
        {
            return _theObject.Validate(_validationContext);
        }
    }
}
