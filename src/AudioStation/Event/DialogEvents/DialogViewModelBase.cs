using SimpleWpf.Extensions;

namespace AudioStation.Event.DialogEvents
{
    public abstract class DialogViewModelBase : ViewModelBase
    {
        /// <summary>
        /// Lets the view know that the result is valid
        /// </summary>
        public bool ResultValid
        {
            get { return Validate(); }
        }

        protected abstract bool Validate();

        public DialogViewModelBase()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName !=  nameof(ResultValid)) 
                    OnPropertyChanged(nameof(ResultValid));
            };
        }
    }
}
