using AudioStation.ViewModels;

namespace AudioStation.Model
{
    public class Artist : ViewModelBase
    {
        string _name;

        public string Name
        {
            get { return _name; }
            set { this.SetProperty(ref _name, value); }
        }

        public Artist()
        {
            this.Name = string.Empty;
        }
    }
}
